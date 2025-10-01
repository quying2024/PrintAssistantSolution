using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;

namespace PrintAssistant.Services;

public class TrayIconService : ITrayIconService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TrayIconService> _logger;
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private bool _disposed;

    public TrayIconService(IServiceProvider serviceProvider, ILogger<TrayIconService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _contextMenu = BuildContextMenu();

        _notifyIcon = new NotifyIcon
        {
            Text = "PrintAssistant",
            Icon = SystemIcons.Application,
            Visible = true,
            ContextMenuStrip = _contextMenu,
        };
    }

    public void UpdateStatus(IEnumerable<PrintJob> recentJobs)
    {
        var jobList = recentJobs.ToList();

        string tooltip = jobList.Count == 0
            ? "打印助手：暂无任务"
            : string.Join(Environment.NewLine, jobList.Take(5).Select(j => $"[{j.Status}] {string.Join(", ", j.SourceFilePaths.Select(Path.GetFileName))}"));

        _notifyIcon.Text = tooltip.Length <= 63 ? tooltip : tooltip[..63];

        _logger.LogDebug("Tray icon status updated with {Count} jobs", jobList.Count);
    }

    public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
        => _notifyIcon.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();

        var settingsItem = new ToolStripMenuItem("设置(&S)", null, OnSettingsClicked);
        var exitItem = new ToolStripMenuItem("退出(&E)", null, OnExitClicked);

        menu.Items.Add(settingsItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        return menu;
    }

    private void OnSettingsClicked(object? sender, EventArgs e)
    {
        using var scope = _serviceProvider.CreateScope();
        var form = scope.ServiceProvider.GetRequiredService<PrintAssistant.UI.SettingsForm>();
        form.ShowDialog();
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        _notifyIcon.Visible = false;
        Application.Exit();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenu.Dispose();
        _disposed = true;
    }
}

