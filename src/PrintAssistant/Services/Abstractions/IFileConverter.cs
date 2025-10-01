namespace PrintAssistant.Services.Abstractions;

public interface IFileConverter
{
    Task<Stream> ConvertToPdfAsync(string sourceFilePath);
}

