using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;
using PrintAssistant.Configuration;
using PrintAssistant.Services.Retry;

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
    private readonly IRetryPolicy _retryPolicy;
    private readonly IJobStageRetryDecider _retryDecider;
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
        IRetryPolicy retryPolicy,
        IJobStageRetryDecider retryDecider,
        IOptions<AppSettings> appSettings)
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
        var context = new RetryContext(job);
        context.Reset();

        var disposableStreams = new List<Stream>();
        Stream? mergedStream = null;

        try
        {
            var pdfStreams = await ExecuteWithRetryAsync(
                context,
                PrintJobStage.Conversion,
                async () => await ConvertSourcesAsync(job, cancellationToken).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
            disposableStreams.AddRange(pdfStreams);

            var mergeResult = await ExecuteWithRetryAsync(
                context,
                PrintJobStage.Merge,
                async () => await MergeAsync(job, pdfStreams).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

            mergedStream = mergeResult.MergedStream;
            disposableStreams.Add(mergedStream);
            job.PageCount = mergeResult.TotalPages;

            await ExecuteWithRetryAsync(
                context,
                PrintJobStage.Print,
                async () => await PrintAsync(job, mergedStream).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);

            await ExecuteWithRetryAsync(
                context,
                PrintJobStage.Archive,
                async () => await ArchiveAsync(job, mergedStream).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
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

    private Task ExecuteWithRetryAsync(RetryContext context, PrintJobStage stage, Func<Task> action, CancellationToken cancellationToken)
        => ExecuteWithRetryAsync(context, stage, async () =>
        {
            await action().ConfigureAwait(false);
            return true;
        }, cancellationToken);

    private async Task<T> ExecuteWithRetryAsync<T>(RetryContext context, PrintJobStage stage, Func<Task<T>> action, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UpdateStatusForStage(context.Job, stage);

            try
            {
                var result = await action().ConfigureAwait(false);
                context.Reset();
                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (TryPrepareRetry(context, stage, ex, out var delay))
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                context.Job.LastFailedStage = stage;
                context.Job.ErrorMessage = ex.Message;
                context.Job.Status = JobStatus.Failed;
                _trayIconService.UpdateStatus(_recentJobs.Values);
                _logger.LogError(ex, "Stage {Stage} failed for job {JobId} with no further retries.", stage, context.Job.JobId);
                throw;
            }
        }
    }

    private bool TryPrepareRetry(RetryContext context, PrintJobStage stage, Exception exception, out TimeSpan delay)
    {
        delay = TimeSpan.Zero;

        if (stage == PrintJobStage.Conversion && exception is InvalidOperationException)
        {
            return false;
        }

        if (!_retryDecider.ShouldRetry(stage))
        {
            return false;
        }

        context.IncrementAttempt(stage, exception.Message ?? exception.GetType().Name);

        var waitTime = _retryPolicy.GetDelay(context.Attempt - 1);
        if (waitTime is null)
        {
            return false;
        }

        context.Job.Status = JobStatus.Retrying;
        _logger.LogWarning(exception, "Stage {Stage} failed for job {JobId}. Will retry attempt {Attempt} after {Delay} ms.", stage, context.Job.JobId, context.Attempt, waitTime.Value.TotalMilliseconds);
        _trayIconService.UpdateStatus(_recentJobs.Values);
        delay = waitTime.Value;
        return true;
    }

    private void UpdateStatusForStage(PrintJob job, PrintJobStage stage)
    {
        switch (stage)
        {
            case PrintJobStage.Conversion:
                job.Status = JobStatus.Converting;
                break;
            case PrintJobStage.Merge:
            case PrintJobStage.Archive:
                job.Status = JobStatus.Processing;
                break;
            case PrintJobStage.Print:
                job.Status = JobStatus.Printing;
                break;
        }

        _trayIconService.UpdateStatus(_recentJobs.Values);
    }

    private async Task<List<Stream>> ConvertSourcesAsync(PrintJob job, CancellationToken cancellationToken)
    {
        var pdfStreams = new List<Stream>();

        try
        {
            foreach (var filePath in job.SourceFilePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var converter = _fileConverterFactory.GetConverter(filePath);
                if (converter == null)
                {
                    _logger.LogWarning("File {FilePath} is not supported and will be moved to unsupported directory.", filePath);
                    _fileArchiver.MoveUnsupportedFile(filePath);
                    continue;
                }

                try
                {
                    var pdfStream = await converter.ConvertToPdfAsync(filePath).ConfigureAwait(false);
                    pdfStreams.Add(pdfStream);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to convert file {FilePath} to PDF.", filePath);
                    throw;
                }
            }

            if (pdfStreams.Count == 0)
            {
                throw new InvalidOperationException("No supported files were available for printing.");
            }

            if (_appSettings.Printing.GenerateCoverPage)
            {
                var coverPageStream = await _coverPageGenerator.GenerateCoverPageAsync(job).ConfigureAwait(false);
                pdfStreams.Insert(0, coverPageStream);
            }

            _logger.LogInformation("Converted {Count} files for job {JobId}.", pdfStreams.Count, job.JobId);
            return pdfStreams;
        }
        catch
        {
            foreach (var stream in pdfStreams)
            {
                stream.Dispose();
            }

            throw;
        }
    }

    private async Task<(Stream MergedStream, int TotalPages)> MergeAsync(PrintJob job, IReadOnlyList<Stream> pdfStreams)
    {
        var result = await _pdfMerger.MergePdfsAsync(pdfStreams).ConfigureAwait(false);
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
}

