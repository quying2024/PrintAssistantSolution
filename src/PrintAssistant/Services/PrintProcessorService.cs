using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;
using PrintAssistant.Configuration;
using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;
using PrintAssistant.Services.Retry;
using PrintAssistant.UI;

namespace PrintAssistant.Services;

public class PrintProcessorService : BackgroundService
{
    private readonly ILogger<PrintProcessorService> _logger;
    private readonly IPrintQueue _printQueue;
    private readonly IFileMonitor _fileMonitor;
    private readonly ITrayIconService _trayIconService;
    private readonly IPrintService _printService;
    private readonly IFileConverterFactory _fileConverterFactory;
    private readonly IPdfMerger _pdfMerger;
    private readonly IFileArchiver _fileArchiver;
    private readonly ICoverPageGenerator _coverPageGenerator;
    private readonly PrintAssistant.Services.Abstractions.IRetryPolicy _retryPolicy;
    private readonly IJobStageRetryDecider _retryDecider;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppSettings _appSettings;

    private readonly ConcurrentDictionary<Guid, PrintJob> _recentJobs = new();

    public PrintProcessorService(
        ILogger<PrintProcessorService> logger,
        IPrintQueue printQueue,
        IFileMonitor fileMonitor,
        ITrayIconService trayIconService,
        IPrintService printService,
        IFileConverterFactory fileConverterFactory,
        IPdfMerger pdfMerger,
        IFileArchiver fileArchiver,
        ICoverPageGenerator coverPageGenerator,
        PrintAssistant.Services.Abstractions.IRetryPolicy retryPolicy,
        IJobStageRetryDecider retryDecider,
        IOptions<AppSettings> appSettings,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _printQueue = printQueue;
        _fileMonitor = fileMonitor;
        _trayIconService = trayIconService;
        _printService = printService;
        _fileConverterFactory = fileConverterFactory;
        _pdfMerger = pdfMerger;
        _fileArchiver = fileArchiver;
        _coverPageGenerator = coverPageGenerator;
        _retryPolicy = retryPolicy;
        _retryDecider = retryDecider;
        _appSettings = appSettings.Value;
        _serviceProvider = serviceProvider;

        _fileMonitor.JobDetected += OnJobDetected;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _fileMonitor.StartMonitoring();
        _trayIconService.ShowBalloonTip(2000, "PrintAssistant", "打印助手已启动，正在监控文件夹", ToolTipIcon.Info);
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _fileMonitor.StopMonitoring();
        return base.StopAsync(cancellationToken);
    }

    private async void OnJobDetected(PrintJob job)
    {
        _recentJobs[job.JobId] = job;
        _trayIconService.UpdateStatus(_recentJobs.Values.OrderByDescending(j => j.CreationTime).Take(5));

        try
        {
            await _printQueue.EnqueueJobAsync(job).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue job {JobId}", job.JobId);
            job.Status = JobStatus.Failed;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            PrintJob? job = null;
            try
            {
                job = await _printQueue.DequeueJobAsync(stoppingToken).ConfigureAwait(false);
                await ProcessJobAsync(job, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (job != null)
                {
                    job.Status = JobStatus.Failed;
                    job.ErrorMessage = ex.Message;
                }

                _logger.LogError(ex, "Error processing print job.");
            }
            finally
            {
                _trayIconService.UpdateStatus(_recentJobs.Values);
            }
        }
    }

    private async Task ProcessJobAsync(PrintJob job, CancellationToken cancellationToken)
    {
        var context = new RetryContext(job, _appSettings.Printing.RetryPolicy);

        var disposableStreams = new List<Stream>();
        Stream? mergedStream = null;
        Stream? archiveStream = null;

        try
        {
            var conversionPolicy = CreateRetryPolicy(PrintJobStage.Conversion);
            var pdfFactories = await conversionPolicy.ExecuteAsync(async () => await ConvertSourcesAsync(job, disposableStreams, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);

            var mergePolicy = CreateRetryPolicy(PrintJobStage.Merge);
            var mergeResult = await mergePolicy.ExecuteAsync(async () => await MergeAsync(job, pdfFactories).ConfigureAwait(false)).ConfigureAwait(false);

            mergedStream = mergeResult.MergedStream;
            disposableStreams.Add(mergedStream);
            job.PageCount = mergeResult.TotalPages;

            archiveStream = new MemoryStream();
            if (mergedStream.CanSeek)
            {
                mergedStream.Position = 0;
            }
            await mergedStream.CopyToAsync(archiveStream).ConfigureAwait(false);
            archiveStream.Position = 0;
            disposableStreams.Add(archiveStream);

            if (!await EnsurePrinterSelectionAsync(job).ConfigureAwait(false))
            {
                _logger.LogInformation("Job {JobId} cancelled by user before printing.", job.JobId);
                job.Status = JobStatus.Cancelled;
                return;
            }

            var printPolicy = CreateRetryPolicy(PrintJobStage.Print);
            await printPolicy.ExecuteAsync(async () => await PrintAsync(job, mergedStream).ConfigureAwait(false)).ConfigureAwait(false);

            var archivePolicy = CreateRetryPolicy(PrintJobStage.Archive);
            await archivePolicy.ExecuteAsync(async () => await ArchiveAsync(job, archiveStream).ConfigureAwait(false)).ConfigureAwait(false);
        }
        finally
        {
            foreach (var stream in disposableStreams)
            {
                stream.Dispose();
            }

            _trayIconService.UpdateStatus(_recentJobs.Values);
        }
    }

    private async Task<List<Func<Task<Stream>>>> ConvertSourcesAsync(PrintJob job, List<Stream> disposableStreams, CancellationToken cancellationToken)
    {
        var factories = new List<Func<Task<Stream>>>();

        foreach (var filePath in job.SourceFilePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var converter = _fileConverterFactory.GetConverter(filePath);
            if (converter == null)
            {
                _logger.LogWarning("File {FilePath} is not supported and will be moved to unsupported directory.", filePath);
                if (job.UnsupportedFilesMoved.Add(filePath))
                {
                    _fileArchiver.MoveUnsupportedFile(filePath);
                }
                continue;
            }

            factories.Add(async () =>
            {
                var pdfStream = await converter.ConvertToPdfAsync(filePath).ConfigureAwait(false);
                disposableStreams.Add(pdfStream);
                return pdfStream;
            });
        }

        if (factories.Count == 0)
        {
            _logger.LogWarning("No supported files found for job {JobId}. Marking as failed.", job.JobId);
            job.Status = JobStatus.Failed;
            job.ErrorMessage = "没有可打印的文件。";
            _trayIconService.UpdateStatus(_recentJobs.Values);
            throw new InvalidOperationException("No supported files were available for printing.");
        }

        if (_appSettings.Printing.GenerateCoverPage)
        {
            factories.Insert(0, async () =>
            {
                var coverStream = await _coverPageGenerator.GenerateCoverPageAsync(job).ConfigureAwait(false);
                disposableStreams.Add(coverStream);
                return coverStream;
            });
        }

        _logger.LogInformation("Prepared {Count} PDF factories for job {JobId}.", factories.Count, job.JobId);
        return factories;
    }

    private async Task<bool> EnsurePrinterSelectionAsync(PrintJob job)
    {
        if (!string.IsNullOrWhiteSpace(job.SelectedPrinter) && job.Copies > 0)
        {
            return true;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dialog = scope.ServiceProvider.GetRequiredService<PrinterSelectionForm>();

            dialog.Initialize(
                _appSettings.Printing.ExcludedPrinters,
                job.SelectedPrinter,
                job.Copies > 0 ? job.Copies : _appSettings.Printing.Windows.DefaultCopies);

            var result = dialog.ShowDialog();

            if (result != DialogResult.OK)
            {
                return false;
            }

            job.SelectedPrinter = dialog.SelectedPrinter;
            job.Copies = Math.Max(1, dialog.PrintCopies);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prompt for printer selection.");
            throw;
        }
    }

    private async Task<(Stream MergedStream, int TotalPages)> MergeAsync(PrintJob job, IReadOnlyList<Func<Task<Stream>>> pdfFactories)
    {
        var result = await _pdfMerger.MergePdfsAsync(pdfFactories).ConfigureAwait(false);
        _logger.LogInformation("Merged PDF for job {JobId} with {PageCount} pages.", job.JobId, result.TotalPages);
        return result;
    }

    private async Task PrintAsync(PrintJob job, Stream mergedStream)
    {
        mergedStream.Position = 0;
        await _printService.PrintPdfAsync(mergedStream, job.SelectedPrinter ?? string.Empty, job.Copies).ConfigureAwait(false);
        job.Status = JobStatus.Completed;
        _logger.LogInformation("Job {JobId} printed successfully with {PageCount} pages.", job.JobId, job.PageCount);
    }

    private async Task ArchiveAsync(PrintJob job, Stream mergedStream)
    {
        mergedStream.Position = 0;
        var archivePath = await _fileArchiver.ArchiveFilesAsync(job.SourceFilePaths, job.CreationTime, mergedStream, $"{job.JobId}.pdf").ConfigureAwait(false);
        job.Status = JobStatus.Archived;
        _logger.LogInformation("Job {JobId} archived at {ArchivePath}.", job.JobId, archivePath);
        _trayIconService.ShowBalloonTip(2000, "打印完成", $"任务 {job.JobId} 已完成并归档。", ToolTipIcon.Info);
    }

    private AsyncRetryPolicy CreateRetryPolicy(PrintJobStage stage)
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: _appSettings.Printing.RetryPolicy.MaxRetryCount,
                sleepDurationProvider: attempt => _retryPolicy.GetDelay(attempt - 1) ?? TimeSpan.Zero,
                onRetry: (exception, timeSpan, attempt, _) =>
                {
                    _logger.LogWarning(exception,
                        "Stage {Stage} failed. Retry attempt {Attempt} after {Delay} ms.",
                        stage,
                        attempt,
                        timeSpan.TotalMilliseconds);
                });
    }
}

