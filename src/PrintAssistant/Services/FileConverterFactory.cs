using PrintAssistant.Services.Abstractions;
using PrintAssistant.Services.Converters;

namespace PrintAssistant.Services;

public class FileConverterFactory(IServiceProvider serviceProvider) : IFileConverterFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IFileConverter? GetConverter(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".doc" or ".docx" => _serviceProvider.GetService(typeof(WordToPdfConverter)) as IFileConverter,
            ".xls" or ".xlsx" or ".xlsm" => _serviceProvider.GetService(typeof(ExcelToPdfConverter)) as IFileConverter,
            ".jpg" or ".jpeg" or ".png" or ".bmp" => _serviceProvider.GetService(typeof(ImageToPdfConverter)) as IFileConverter,
            ".pdf" => new PassthroughConverter(),
            _ => null,
        };
    }

    private class PassthroughConverter : IFileConverter
    {
        public async Task<Stream> ConvertToPdfAsync(string sourceFilePath)
        {
            var memoryStream = new MemoryStream();
            await using var fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}

