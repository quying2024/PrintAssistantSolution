using PrintAssistant.Services.Abstractions;

namespace PrintAssistant.Services;

public class PrintService : IPrintService
{
    public Task PrintPdfAsync(Stream pdfStream, string printerName, int copies)
    {
        // TODO: Implement silent printing in subsequent iterations.
        return Task.CompletedTask;
    }
}

