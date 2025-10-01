namespace PrintAssistant.Services.Abstractions;

public interface IFileConverterFactory
{
    IFileConverter? GetConverter(string filePath);
}

