using PrintAssistant.Services.Abstractions;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using System.IO.Abstractions;

namespace PrintAssistant.Services.Converters;

public class ExcelToPdfConverter(IFileSystem fileSystem) : IFileConverter
{
    private readonly IFileSystem _fileSystem = fileSystem;

    public async Task<Stream> ConvertToPdfAsync(string sourceFilePath)
    {
        using var excelEngine = new ExcelEngine();
        var application = excelEngine.Excel;
        await using var fileStream = _fileSystem.File.OpenRead(sourceFilePath);

        var workbook = application.Workbooks.Open(fileStream);
        var renderer = new XlsIORenderer();

        var settings = new XlsIORendererSettings
        {
            LayoutOptions = LayoutOptions.FitAllColumnsOnOnePage
        };

        var pdfDocument = renderer.ConvertToPDF(workbook, settings);
        var pdfStream = new MemoryStream();
        pdfDocument.Save(pdfStream);
        pdfStream.Position = 0;
        return pdfStream;
    }
}

