using System.Windows.Forms;
using PrintAssistant.Core;

namespace PrintAssistant.Services.Abstractions;

/// <summary>
/// 托盘图标服务接口，用于管理托盘图标及其交互。
/// </summary>
public interface ITrayIconService
{
    /// <summary>
    /// 更新托盘图标提示信息。
    /// </summary>
    void UpdateStatus(IEnumerable<PrintJob> recentJobs);

    /// <summary>
    /// 显示气泡提示。
    /// </summary>
    void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon);
}

