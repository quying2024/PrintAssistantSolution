using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;

namespace PrintAssistant.Services;

public class PrintQueueService : IPrintQueue
{
    private readonly BufferBlock<PrintJob> _queue;
    private readonly ILogger<PrintQueueService> _logger;

    public PrintQueueService(ILogger<PrintQueueService> logger)
    {
        _logger = logger;
        _queue = new BufferBlock<PrintJob>(new DataflowBlockOptions
        {
            BoundedCapacity = DataflowBlockOptions.Unbounded
        });
    }

    public async Task EnqueueJobAsync(PrintJob job)
    {
        if (job == null)
        {
            throw new ArgumentNullException(nameof(job));
        }

        await _queue.SendAsync(job).ConfigureAwait(false);
        _logger.LogInformation("Job {JobId} enqueued.", job.JobId);
    }

    public Task<PrintJob> DequeueJobAsync(CancellationToken cancellationToken) => _queue.ReceiveAsync(cancellationToken);

    public IReceivableSourceBlock<PrintJob> AsReceivableSourceBlock() => _queue;

    public void ReleaseJob(PrintJob job)
    {
        if (job.LastFailedStage == null)
        {
            return;
        }

        _logger.LogWarning("Releasing job {JobId} back to queue after failure at stage {Stage}.", job.JobId, job.LastFailedStage);
        _queue.Post(job);
    }
}

