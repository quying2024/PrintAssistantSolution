using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;

namespace PrintAssistant.Services;

public class PrintProcessorService : BackgroundService
{
    private readonly ILogger<PrintProcessorService> _logger;
    private readonly IPrintQueue _printQueue;
    private readonly IPrintService _printService;

    public PrintProcessorService(ILogger<PrintProcessorService> logger, IPrintQueue printQueue, IPrintService printService)
    {
        _logger = logger;
        _printQueue = printQueue;
        _printService = printService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _printQueue.DequeueJobAsync(stoppingToken);
                await _printService.PrintPdfAsync(Stream.Null, job.SelectedPrinter ?? string.Empty, job.Copies);
                job.Status = JobStatus.Completed;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing print job");
            }
        }
    }
}

