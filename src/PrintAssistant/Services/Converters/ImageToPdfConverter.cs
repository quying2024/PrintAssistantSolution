using PrintAssistant.Services.Abstractions;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System.IO.Abstractions;

namespace PrintAssistant.Services.Converters;

public class ImageToPdfConverter(IFileSystem fileSystem) : IFileConverter
{
    private readonly IFileSystem _fileSystem = fileSystem;

    public async Task<Stream> ConvertToPdfAsync(string sourceFilePath)
    {
        using var document = new PdfDocument();
        var page = document.Pages.Add();

        await using var fileStream = _fileSystem.File.OpenRead(sourceFilePath);
        var image = new PdfBitmap(fileStream);
        page.Graphics.DrawImage(image, new Syncfusion.Drawing.PointF(0, 0));

        var pdfStream = new MemoryStream();
        document.Save(pdfStream);
        pdfStream.Position = 0;
        return pdfStream;
    }
}

