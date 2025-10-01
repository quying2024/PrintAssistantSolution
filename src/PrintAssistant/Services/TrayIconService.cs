using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;

namespace PrintAssistant.Services;

public class TrayIconService : ITrayIconService
{
    public void UpdateStatus(IEnumerable<PrintJob> recentJobs)
    {
        // TODO: Implement tray icon status updates in subsequent iterations.
    }

    public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
    {
        // TODO: Implement tray icon balloon tips in subsequent iterations.
    }
}

