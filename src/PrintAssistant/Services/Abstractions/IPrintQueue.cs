using PrintAssistant.Core;
using System.Threading.Tasks.Dataflow;

namespace PrintAssistant.Services.Abstractions;

public interface IPrintQueue
{
    Task EnqueueJobAsync(PrintJob job);
    Task<PrintJob> DequeueJobAsync(CancellationToken cancellationToken);
    IReceivableSourceBlock<PrintJob> AsReceivableSourceBlock();
    void ReleaseJob(PrintJob job);
}

