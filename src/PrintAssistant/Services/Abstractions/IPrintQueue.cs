using PrintAssistant.Core;
using System.Threading.Tasks.Dataflow;

namespace PrintAssistant.Services.Abstractions;

/// <summary>
/// 打印队列接口，负责在生产者与消费者之间传递打印任务。
/// </summary>
public interface IPrintQueue
{
    /// <summary>
    /// 将打印任务入队。
    /// </summary>
    Task EnqueueJobAsync(PrintJob job);

    /// <summary>
    /// 从队列中取出打印任务。
    /// </summary>
    Task<PrintJob> DequeueJobAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 以数据流块的形式暴露队列。
    /// </summary>
    IReceivableSourceBlock<PrintJob> AsReceivableSourceBlock();
}

