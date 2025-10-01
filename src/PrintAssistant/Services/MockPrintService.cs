using Microsoft.Extensions.Logging;
using PrintAssistant.Services.Abstractions;

namespace PrintAssistant.Services;

/// <summary>
/// IPrintService 的模拟实现，用于迭代 1。
/// 它不执行任何实际的打印操作，仅记录一条日志并模拟成功。
/// </summary>
public class MockPrintService : IPrintService
{
    private readonly ILogger<MockPrintService> _logger;

    public MockPrintService(ILogger<MockPrintService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 模拟打印一个PDF流。
    /// </summary>
    /// <param name="pdfStream">要打印的PDF流。</param>
    /// <param name="printerName">目标打印机名称。</param>
    /// <param name="copies">打印份数。</param>
    /// <returns>一个表示异步操作的任务。</returns>
    public async Task PrintPdfAsync(Stream pdfStream, string printerName, int copies)
    {
        _logger.LogInformation(
            "MockPrintService: 模拟打印任务。打印机: {PrinterName}, 份数: {Copies}, 数据流大小: {StreamLength} bytes.",
            printerName,
            copies,
            pdfStream.Length);

        await Task.Delay(1000).ConfigureAwait(false);
    }
}

