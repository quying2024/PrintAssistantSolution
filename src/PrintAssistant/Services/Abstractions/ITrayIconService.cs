using System.Windows.Forms;
using PrintAssistant.Core;

namespace PrintAssistant.Services.Abstractions;

public interface ITrayIconService
{
    void UpdateStatus(IEnumerable<PrintJob> recentJobs);
    void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon);
}

