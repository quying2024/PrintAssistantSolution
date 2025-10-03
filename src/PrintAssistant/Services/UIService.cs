using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;
using PrintAssistant.UI;
using System.Windows.Forms;

namespace PrintAssistant.Services;

/// <summary>
/// UI服务的实现，负责在正确的UI线程上创建和显示窗体。
/// </summary>
public class UIService : IUIService, IDisposable
{
    private readonly IOptions<AppSettings> _appSettings;
    private readonly Control _invoker; // 一个隐藏的控件，用于访问UI线程
    private bool _disposed;

    public UIService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        
        // 创建一个隐藏的控件，它的句柄将在主UI线程上创建。
        // 我们用它来安全地调用Invoke。
        _invoker = new Control();
        _invoker.CreateControl();
    }

    public Task<bool> ShowPrinterSelectionDialogAsync(PrintJob job)
    {
        if (job == null)
        {
            throw new ArgumentNullException(nameof(job));
        }

        // 使用 TaskCompletionSource 在后台线程中等待UI线程的结果
        var tcs = new TaskCompletionSource<bool>();

        // 将显示对话框的操作封送到主UI线程执行
        _invoker.Invoke(() =>
        {
            try
            {
                using var dialog = new PrinterSelectionForm();
                
                dialog.Initialize(
                    _appSettings.Value.Printing.ExcludedPrinters,
                    job.SelectedPrinter,
                    job.Copies > 0 ? job.Copies : _appSettings.Value.Printing.Windows.DefaultCopies);

                // 关键步骤：将隐藏的控件作为对话框的所有者
                // 这能极大地提高置顶的成功率
                var result = dialog.ShowDialog(_invoker);

                if (result == DialogResult.OK)
                {
                    job.SelectedPrinter = dialog.SelectedPrinter;
                    job.Copies = dialog.PrintCopies;
                    tcs.SetResult(true);
                }
                else
                {
                    tcs.SetResult(false);
                }
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _invoker?.Dispose();
        _disposed = true;
    }
}
