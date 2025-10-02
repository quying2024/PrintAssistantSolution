using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using PrintAssistant.Services.Abstractions;

namespace PrintAssistant.Services;

/// <summary>
/// Windows 平台打印服务占位实现，当前仅记录日志和校验配置。
/// 后续迭代将补充实际 PDF 渲染与打印逻辑。
/// </summary>
public class WindowsPrintService : IPrintService
{
    private readonly ILogger<WindowsPrintService> _logger;
    private readonly WindowsPrintSettings _settings;

    public WindowsPrintService(ILogger<WindowsPrintService> logger, IOptions<AppSettings> options)
    {
        _logger = logger;
        _settings = options.Value.Printing.Windows;
    }

    public Task PrintPdfAsync(Stream pdfStream, string printerName, int copies)
    {
        if (pdfStream == null)
        {
            throw new ArgumentNullException(nameof(pdfStream));
        }

        var targetPrinter = string.IsNullOrWhiteSpace(printerName) ? _settings.DefaultPrinter : printerName;
        if (string.IsNullOrWhiteSpace(targetPrinter))
        {
            throw new InvalidOperationException("WindowsPrintService: 未指定打印机，请检查配置或用户输入。");
        }

        var effectiveCopies = copies > 0 ? copies : Math.Max(1, _settings.DefaultCopies);

        _logger.LogInformation(
            "WindowsPrintService: 模拟打印任务。打印机: {Printer}, 份数: {Copies}, PDF 长度: {Length} 字节。",
            targetPrinter,
            effectiveCopies,
            pdfStream.CanSeek ? pdfStream.Length : -1);

        return Task.CompletedTask;
    }
}

