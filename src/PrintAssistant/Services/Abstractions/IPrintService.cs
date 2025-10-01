namespace PrintAssistant.Services.Abstractions;

public interface IPrintService
{
    Task PrintPdfAsync(Stream pdfStream, string printerName, int copies);
}

