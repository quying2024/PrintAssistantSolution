using PrintAssistant.Services.Abstractions;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using System.IO.Abstractions;

namespace PrintAssistant.Services.Converters;

public class WordToPdfConverter(IFileSystem fileSystem) : IFileConverter
{
    private readonly IFileSystem _fileSystem = fileSystem;

    public async Task<Stream> ConvertToPdfAsync(string sourceFilePath)
    {
        await using var fileStream = _fileSystem.File.OpenRead(sourceFilePath);
        using var wordDocument = new WordDocument(fileStream, FormatType.Automatic);
        using var renderer = new DocIORenderer();

        var pdfDocument = renderer.ConvertToPDF(wordDocument);
        var pdfStream = new MemoryStream();
        pdfDocument.Save(pdfStream);
        pdfStream.Position = 0;
        return pdfStream;
    }
}

