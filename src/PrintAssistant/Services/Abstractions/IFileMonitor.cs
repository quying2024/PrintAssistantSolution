using PrintAssistant.Core;

namespace PrintAssistant.Services.Abstractions;

/// <summary>
/// 文件监控服务接口，负责监控指定目录并在检测到打印任务时触发事件。
/// </summary>
public interface IFileMonitor
{
    /// <summary>
    /// 当检测到新的打印任务时触发。
    /// </summary>
    event Action<PrintJob>? JobDetected;

    /// <summary>
    /// 启动监控。
    /// </summary>
    void StartMonitoring();

    /// <summary>
    /// 停止监控。
    /// </summary>
    void StopMonitoring();
}

