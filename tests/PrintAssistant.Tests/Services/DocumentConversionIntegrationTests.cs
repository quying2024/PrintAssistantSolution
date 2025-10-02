using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using PrintAssistant.Services;
using ImageToPdfConverter = PrintAssistant.Services.Converters.ImageToPdfConverter;
using WordToPdfConverter = PrintAssistant.Services.Converters.WordToPdfConverter;
using ExcelToPdfConverter = PrintAssistant.Services.Converters.ExcelToPdfConverter;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using Xunit;
using Xunit.Abstractions;
using FileSystem = System.IO.Abstractions.FileSystem;

namespace PrintAssistant.Tests.Services;

public class DocumentConversionIntegrationTests : IDisposable
{
    private static bool _licenseRegistered;

    private readonly string _workingDirectory;
    private readonly ITestOutputHelper _output;
    private readonly FileSystem _fileSystem = new();

    public DocumentConversionIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        _workingDirectory = Path.Combine(Path.GetTempPath(), "PrintAssistantConversionTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workingDirectory);

        RegisterSyncfusionLicense();
    }

    [Fact]
    public async Task ConvertVariousDocuments_ToPdfStreams_AllValid()
    {
        var wordPath = CreateSampleWordDocument();
        var excelPath = CreateSampleExcelWorkbook();
        var imagePath = CreateSampleImage();

        await using var wordPdf = await new WordToPdfConverter(_fileSystem).ConvertToPdfAsync(wordPath);
        await using var excelPdf = await new ExcelToPdfConverter(_fileSystem).ConvertToPdfAsync(excelPath);
        await using var imagePdf = await new ImageToPdfConverter(_fileSystem).ConvertToPdfAsync(imagePath);

        var pdfStreams = new List<Stream> { CloneStream(wordPdf), CloneStream(excelPdf), CloneStream(imagePdf) };

        foreach (var pdfStream in pdfStreams)
        {
            pdfStream.Position = 0;
            using var loadedDocument = new PdfLoadedDocument(pdfStream);
            Assert.True(loadedDocument.PageCount >= 1);
        }

        var merger = new PdfMerger();
        var (mergedStream, totalPages) = await merger.MergePdfsAsync(pdfStreams);

        Assert.True(totalPages >= pdfStreams.Count);
        using var mergedDocument = new PdfLoadedDocument(mergedStream);
        Assert.Equal(totalPages, mergedDocument.PageCount);
    }

    [Fact]
    public async Task PdfMerge_PerformanceBaseline_LogsMetrics()
    {
        const int documentCount = 20;

        var pdfStreams = new List<Stream>();
        for (var i = 0; i < documentCount; i++)
        {
            pdfStreams.Add(await CreateSyntheticPdfStreamAsync($"Sample Document #{i + 1}"));
        }

        var merger = new PdfMerger();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memoryBefore = GC.GetTotalMemory(forceFullCollection: true);
        var stopwatch = Stopwatch.StartNew();
        var (mergedStream, totalPages) = await merger.MergePdfsAsync(pdfStreams);
        stopwatch.Stop();
        var memoryAfter = GC.GetTotalMemory(forceFullCollection: true);

        using var mergedDocument = new PdfLoadedDocument(mergedStream);

        Assert.True(totalPages >= documentCount);
        Assert.Equal(totalPages, mergedDocument.PageCount);

        _output.WriteLine($"Merged {documentCount} PDFs into {totalPages} pages.");
        _output.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        _output.WriteLine($"Approx. memory delta: {(memoryAfter - memoryBefore) / 1024.0 / 1024.0:F2} MB.");

        foreach (var stream in pdfStreams)
        {
            stream.Dispose();
        }
    }

    private string CreateSampleWordDocument()
    {
        var filePath = Path.Combine(_workingDirectory, "sample.docx");

        using var document = new WordDocument();
        var section = document.AddSection();
        var paragraph = section.AddParagraph();
        paragraph.AppendText("PrintAssistant 文档转换端到端验证 - Word");

        document.Save(filePath, FormatType.Docx);

        return filePath;
    }

    private string CreateSampleExcelWorkbook()
    {
        var filePath = Path.Combine(_workingDirectory, "sample.xlsx");

        using var excelEngine = new ExcelEngine();
        var workbook = excelEngine.Excel.Workbooks.Create(1);
        var sheet = workbook.Worksheets[0];
        sheet.Range["A1"].Text = "PrintAssistant";
        sheet.Range["A2"].Text = "文档转换端到端验证 - Excel";
        sheet.Range["B2"].Number = 2025;

        workbook.SaveAs(filePath);

        return filePath;
    }

    private string CreateSampleImage()
    {
        var filePath = Path.Combine(_workingDirectory, "sample.png");

        using var bitmap = new System.Drawing.Bitmap(400, 200);
        using var graphics = System.Drawing.Graphics.FromImage(bitmap);
        graphics.Clear(System.Drawing.Color.LightSteelBlue);
        graphics.DrawString("PrintAssistant", new System.Drawing.Font("Arial", 24), System.Drawing.Brushes.Black, new System.Drawing.PointF(20, 60));
        graphics.DrawString("文档转换端到端验证 - 图片", new System.Drawing.Font("Arial", 12), System.Drawing.Brushes.Black, new System.Drawing.PointF(20, 110));

        bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

        return filePath;
    }

    private async Task<Stream> CreateSyntheticPdfStreamAsync(string title)
    {
        using var document = new PdfDocument();
        var page = document.Pages.Add();
        var graphics = page.Graphics;

        var headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 18, PdfFontStyle.Bold);
        var bodyFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12);

        graphics.DrawString(title, headerFont, PdfBrushes.DarkBlue, new Syncfusion.Drawing.PointF(40, 40));
        graphics.DrawString("生成时间: " + DateTime.Now.ToString("F"), bodyFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(40, 80));

        var stream = new MemoryStream();
        document.Save(stream);
        await stream.FlushAsync().ConfigureAwait(false);
        stream.Position = 0;
        document.Close(true);

        return stream;
    }

    private static Stream CloneStream(Stream source)
    {
        source.Position = 0;
        var clone = new MemoryStream();
        source.CopyTo(clone);
        clone.Position = 0;
        source.Position = 0;
        return clone;
    }

    private static void RegisterSyncfusionLicense()
    {
        if (_licenseRegistered)
        {
            return;
        }

        var root = FindSolutionRoot();
        if (root == null)
        {
            throw new InvalidOperationException("无法定位解决方案根目录以加载 Syncfusion 许可证。");
        }

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(root)
            .AddJsonFile(Path.Combine("src", "PrintAssistant", "appsettings.Secret.json"), optional: true, reloadOnChange: false);

        var configuration = configBuilder.Build();
        var licenseKeys = configuration.GetSection("Syncfusion:DocumentSdk:LicenseKeys").Get<string[]>() ?? Array.Empty<string>();

        foreach (var key in licenseKeys)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(key);
            }
        }

        _licenseRegistered = true;
    }

    private static string? FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null && !directory.GetFiles("PrintAssistantSolution.sln").Any())
        {
            directory = directory.Parent;
        }

        return directory?.FullName;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_workingDirectory))
            {
                Directory.Delete(_workingDirectory, recursive: true);
            }
        }
        catch
        {
            // 忽略清理异常，避免影响测试结果。
        }
    }
}

