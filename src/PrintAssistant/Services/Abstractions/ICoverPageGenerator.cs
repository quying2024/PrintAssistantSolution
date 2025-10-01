using PrintAssistant.Core;

namespace PrintAssistant.Services.Abstractions;

public interface ICoverPageGenerator
{
    Task<Stream> GenerateCoverPageAsync(PrintJob job);
}

