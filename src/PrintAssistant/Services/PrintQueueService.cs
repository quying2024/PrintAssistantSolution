using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;
using System.Threading.Tasks.Dataflow;

namespace PrintAssistant.Services;

public class PrintQueueService : IPrintQueue
{
    private readonly BufferBlock<PrintJob> _queue = new();

    public Task EnqueueJobAsync(PrintJob job) => _queue.SendAsync(job);

    public Task<PrintJob> DequeueJobAsync(CancellationToken cancellationToken) => _queue.ReceiveAsync(cancellationToken);

    public IReceivableSourceBlock<PrintJob> AsReceivableSourceBlock() => _queue;
}

