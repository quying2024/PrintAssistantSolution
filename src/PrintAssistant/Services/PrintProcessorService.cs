using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;

namespace PrintAssistant.Services;

public class PrintProcessorService : BackgroundService
{
    private readonly ILogger<PrintProcessorService> _logger;
    private readonly IPrintQueue _printQueue;
    private readonly IFileMonitor _fileMonitor;
    private readonly ITrayIconService _trayIconService;
    private readonly IPrintService _printService;
    private readonly IServiceProvider _serviceProvider;

    private readonly ConcurrentDictionary<Guid, PrintJob> _recentJobs = new();

    public PrintProcessorService(
        ILogger<PrintProcessorService> logger,
        IPrintQueue printQueue,
        IFileMonitor fileMonitor,
        ITrayIconService trayIconService,
        IPrintService printService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _printQueue = printQueue;
        _fileMonitor = fileMonitor;
        _trayIconService = trayIconService;
        _printService = printService;
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
                job.Status = JobStatus.Processing;
                _trayIconService.UpdateStatus(_recentJobs.Values);

                // TODO: 在后续迭代中加入文件转换和 PDF 合并逻辑
                await _printService.PrintPdfAsync(Stream.Null, job.SelectedPrinter ?? string.Empty, job.Copies).ConfigureAwait(false);

                job.Status = JobStatus.Completed;
                _logger.LogInformation("Job {JobId} completed.", job.JobId);
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
}

