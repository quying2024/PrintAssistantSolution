using PrintAssistant.Core;

namespace PrintAssistant.Services.Abstractions;

/// <summary>
/// 提供在主UI线程上执行操作的服务。
/// </summary>
public interface IUIService
{
    /// <summary>
    /// 在主UI线程上异步显示打印机选择对话框。
    /// </summary>
    /// <param name="job">当前的打印任务。</param>
    /// <returns>一个表示对话框结果的任务，如果用户确认则为 true。</returns>
    Task<bool> ShowPrinterSelectionDialogAsync(PrintJob job);
}
