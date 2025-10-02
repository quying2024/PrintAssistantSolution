using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Printing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using PrintAssistant.Services.Abstractions;
using Syncfusion.Pdf.Parsing;
using Syncfusion.PdfToImageConverter;

namespace PrintAssistant.Services;

/// <summary>
/// Windows 平台打印服务，实现 PDF -> 位图 -> 打印机的转换流程，并具备重试与资源清理。
/// </summary>
public class WindowsPrintService : IPrintService, IDisposable
{
    private readonly ILogger<WindowsPrintService> _logger;
    private readonly WindowsPrintSettings _settings;
    private readonly ConcurrentDictionary<int, Bitmap> _pageCache = new();
    private readonly object _printLock = new();
    private bool _disposed;

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

        if (!pdfStream.CanSeek)
        {
            throw new InvalidOperationException("打印所需的 PDF 数据流必须支持定位。");
        }

        lock (_printLock)
        {
            pdfStream.Position = 0;

            var targetPrinter = string.IsNullOrWhiteSpace(printerName) ? _settings.DefaultPrinter : printerName;
            if (string.IsNullOrWhiteSpace(targetPrinter))
            {
                throw new InvalidOperationException("WindowsPrintService: 未指定打印机，请检查配置或用户输入。");
            }

            var verifiedPrinter = ValidatePrinter(targetPrinter);
            var effectiveCopies = copies > 0 ? copies : Math.Max(1, _settings.DefaultCopies);

            using var converter = new PdfToImageConverter();
            converter.Load(pdfStream);

            var pageCount = converter.PageCount;
            if (pageCount == 0)
            {
                throw new InvalidOperationException("PDF 文档为空，无法打印。");
            }

            _logger.LogInformation(
                "WindowsPrintService: 开始打印任务，共 {PageCount} 页，目标打印机 {Printer}，份数 {Copies}。",
                pageCount,
                verifiedPrinter,
                effectiveCopies);

            PreloadPages(converter, pageCount);

            using var document = new PrintDocument
            {
                PrintController = new StandardPrintController(),
                PrinterSettings = CreatePrinterSettings(verifiedPrinter, effectiveCopies)
            };

            ConfigurePageSettings(document.DefaultPageSettings);

            var currentPage = 0;
            PrintPageEventHandler? handler = null;
            handler = (_, e) =>
            {
                if (!_pageCache.TryGetValue(currentPage, out var bitmap) || bitmap == null)
                {
                    _logger.LogWarning("WindowsPrintService: 第 {Page} 页的位图数据不存在，将跳过剩余页面。", currentPage + 1);
                    e.HasMorePages = false;
                    return;
                }

                DrawImage(e, bitmap);
                currentPage++;
                e.HasMorePages = currentPage < pageCount;
            };

            document.PrintPage += handler;

            try
            {
                document.Print();
                _logger.LogInformation("WindowsPrintService: 打印任务已提交，打印机 {Printer}。", verifiedPrinter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WindowsPrintService: 打印任务失败。打印机 {Printer}，错误：{Message}", verifiedPrinter, ex.Message);
                throw;
            }
            finally
            {
                if (handler != null)
                {
                    document.PrintPage -= handler;
                }

                ClearCachedBitmaps();
            }
        }

        return Task.CompletedTask;
    }

    private string ValidatePrinter(string printerName)
    {
        var installed = PrinterSettings.InstalledPrinters.Cast<string>()
            .FirstOrDefault(p => p.Equals(printerName, StringComparison.OrdinalIgnoreCase));

        if (installed == null)
        {
            throw new InvalidOperationException($"打印机 '{printerName}' 不存在或不可用。");
        }

        return installed;
    }

    private void PreloadPages(PdfToImageConverter converter, int pageCount)
    {
        _pageCache.Clear();

        for (var index = 0; index < pageCount; index++)
        {
            using var pageStream = new MemoryStream();
            using var image = converter.ExportAsImage(index, _settings.Dpi ?? 200);
            if (image == null)
            {
                _logger.LogWarning("WindowsPrintService: 无法将第 {Page} 页转换为位图。", index + 1);
                continue;
            }

            image.Save(pageStream, System.Drawing.Imaging.ImageFormat.Png);
            pageStream.Position = 0;

            using var bitmap = new Bitmap(pageStream);
            _pageCache[index] = new Bitmap(bitmap);
        }
    }

    private PrinterSettings CreatePrinterSettings(string printerName, int requestedCopies)
    {
        var settings = new PrinterSettings
        {
            PrinterName = printerName,
            Copies = (short)Math.Clamp(requestedCopies, 1, short.MaxValue),
            Collate = _settings.Collate,
            Duplex = _settings.Duplex ? Duplex.Vertical : Duplex.Simplex,
        };

        if (!string.IsNullOrWhiteSpace(_settings.PaperSource))
        {
            var match = settings.PaperSources.Cast<PaperSource>()
                .FirstOrDefault(ps => ps.SourceName.Equals(_settings.PaperSource, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                settings.DefaultPageSettings.PaperSource = match;
            }
        }

        return settings;
    }

    private void ConfigurePageSettings(PageSettings pageSettings)
    {
        pageSettings.Landscape = _settings.Landscape;
        pageSettings.Color = _settings.Color;
    }

    private void DrawImage(PrintPageEventArgs e, Bitmap image)
    {
        if (_settings.StretchToFit)
        {
            e.Graphics.DrawImage(image, e.MarginBounds);
            return;
        }

        var ratio = Math.Min((double)e.MarginBounds.Width / image.Width, (double)e.MarginBounds.Height / image.Height);
        var scaledWidth = (int)(image.Width * ratio);
        var scaledHeight = (int)(image.Height * ratio);

        var offsetX = e.MarginBounds.Left + (e.MarginBounds.Width - scaledWidth) / 2;
        var offsetY = e.MarginBounds.Top + (e.MarginBounds.Height - scaledHeight) / 2;

        e.Graphics.DrawImage(image, new Rectangle(offsetX, offsetY, scaledWidth, scaledHeight));
    }

    private void ClearCachedBitmaps()
    {
        foreach (var bitmap in _pageCache.Values)
        {
            bitmap.Dispose();
        }

        _pageCache.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        ClearCachedBitmaps();
        _disposed = true;
    }
}

