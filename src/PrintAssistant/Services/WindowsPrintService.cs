using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using PrintAssistant.Services.Abstractions;
using Syncfusion.PdfToImageConverter;

namespace PrintAssistant.Services;

/// <summary>
/// Windows 平台打印服务实现，使用 Syncfusion.PdfToImageConverter 将 PDF 转换为图像并通过 GDI 打印。
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

    public async Task PrintPdfAsync(Stream pdfStream, string printerName, int copies)
    {
        if (pdfStream == null)
        {
            throw new ArgumentNullException(nameof(pdfStream));
        }

        var targetPrinter = string.IsNullOrWhiteSpace(printerName)
            ? _settings.DefaultPrinter
            : printerName;

        if (string.IsNullOrWhiteSpace(targetPrinter))
        {
            throw new InvalidOperationException("WindowsPrintService: 未指定打印机，请检查配置或用户输入。");
        }

        var effectiveCopies = copies > 0 ? copies : Math.Max(1, _settings.DefaultCopies);

        if (pdfStream.CanSeek)
        {
            pdfStream.Position = 0;
        }

        try
        {
            using var converter = new PdfToImageConverter();
            converter.Load(pdfStream);

            var pageCount = converter.PageCount;
            if (pageCount <= 0)
            {
                _logger.LogWarning("WindowsPrintService: 文档页数为 0，无法打印。打印机: {Printer}", targetPrinter);
                return;
            }

            var rawStreams = converter.Convert(0, pageCount - 1, keepTransparency: false, isSkipAnnotations: false);
            if (rawStreams == null || rawStreams.Length == 0)
            {
                _logger.LogWarning("WindowsPrintService: PDF 转换为空，跳过打印。打印机: {Printer}", targetPrinter);
                return;
            }

            var imageStreams = new MemoryStream?[rawStreams.Length];
            for (var i = 0; i < rawStreams.Length; i++)
            {
                if (rawStreams[i] == null)
                {
                    continue;
                }

                var memory = new MemoryStream();
                rawStreams[i].Position = 0;
                rawStreams[i].CopyTo(memory);
                memory.Position = 0;
                imageStreams[i] = memory;
                rawStreams[i].Dispose();
            }

            using var printDocument = new PrintDocument
            {
                PrinterSettings = new PrinterSettings
                {
                    PrinterName = targetPrinter,
                    Copies = (short)Math.Clamp(effectiveCopies, 1, short.MaxValue)
                }
            };

            var paperSize = printDocument.PrinterSettings.PaperSizes
                .Cast<PaperSize?>()
                .FirstOrDefault(p => p?.Kind == PaperKind.A4);

            if (paperSize == null)
            {
                paperSize = new PaperSize("A4", width: 827, height: 1169);
            }

            printDocument.DefaultPageSettings.PaperSize = paperSize;
            printDocument.PrinterSettings.DefaultPageSettings.PaperSize = paperSize;

            printDocument.PrintController = new StandardPrintController();

            var totalPages = imageStreams.Length;
            var currentPage = 0;

            printDocument.PrintPage += (sender, e) =>
            {
                if (currentPage < totalPages && imageStreams[currentPage] != null)
                {
                    try
                    {
                        var stream = imageStreams[currentPage]!;

                        if (stream.CanSeek)
                        {
                            stream.Position = 0;
                        }

                        using var image = Image.FromStream(stream, useEmbeddedColorManagement: true, validateImageData: true);
                        DrawImage(e.Graphics!, image, e.MarginBounds);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "WindowsPrintService: 打印第 {Page} 页时发生错误。", currentPage + 1);
                    }
                    finally
                    {
                        imageStreams[currentPage]?.Dispose();
                        imageStreams[currentPage] = null;
                    }
                }

                currentPage++;
                e.HasMorePages = currentPage < totalPages;
            };

            await Task.Run(() => printDocument.Print()).ConfigureAwait(false);

            _logger.LogInformation(
                "WindowsPrintService: 成功打印 PDF。打印机: {Printer}, 份数: {Copies}, 页数: {Pages}",
                targetPrinter,
                effectiveCopies,
                totalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WindowsPrintService: 打印过程中发生异常。");
            throw;
        }
    }

    private static void DrawImage(Graphics graphics, Image image, Rectangle targetBounds)
    {
        if (graphics == null || image == null)
        {
            return;
        }

        var scaleX = (float)targetBounds.Width / image.Width;
        var scaleY = (float)targetBounds.Height / image.Height;
        var scale = Math.Min(scaleX, scaleY);

        var drawWidth = (int)(image.Width * scale);
        var drawHeight = (int)(image.Height * scale);
        var offsetX = targetBounds.X + (targetBounds.Width - drawWidth) / 2;
        var offsetY = targetBounds.Y + (targetBounds.Height - drawHeight) / 2;

        graphics.DrawImage(image, offsetX, offsetY, drawWidth, drawHeight);
    }
}

