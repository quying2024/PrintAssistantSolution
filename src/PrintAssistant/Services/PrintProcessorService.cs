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
        job.Status = JobStatus.Converting;
        _trayIconService.UpdateStatus(_recentJobs.Values);

        var pdfStreams = new List<Stream>();
        var disposableStreams = new List<Stream>();
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
                    disposableStreams.Add(pdfStream);
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
                disposableStreams.Add(coverPageStream);
            }

            var (mergedStream, totalPages) = await _pdfMerger.MergePdfsAsync(pdfStreams).ConfigureAwait(false);
            disposableStreams.Add(mergedStream);

            job.PageCount = totalPages;
            job.Status = JobStatus.Printing;
            _trayIconService.UpdateStatus(_recentJobs.Values);

            mergedStream.Position = 0;
            await _printService.PrintPdfAsync(mergedStream, job.SelectedPrinter ?? string.Empty, job.Copies).ConfigureAwait(false);

            job.Status = JobStatus.Completed;
            _logger.LogInformation("Job {JobId} printed successfully with {PageCount} pages.", job.JobId, job.PageCount);

            mergedStream.Position = 0;
            var archivePath = await _fileArchiver.ArchiveFilesAsync(job.SourceFilePaths, job.CreationTime, mergedStream, $"{job.JobId}.pdf").ConfigureAwait(false);
            job.Status = JobStatus.Archived;
            _logger.LogInformation("Job {JobId} archived at {ArchivePath}.", job.JobId, archivePath);

            _trayIconService.ShowBalloonTip(2000, "打印完成", $"任务 {job.JobId} 已完成并归档。", ToolTipIcon.Info);
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
}

