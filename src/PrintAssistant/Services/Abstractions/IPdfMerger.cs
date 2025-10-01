namespace PrintAssistant.Services.Abstractions;

public interface IPdfMerger
{
    Task<(Stream MergedPdfStream, int TotalPages)> MergePdfsAsync(IEnumerable<Stream> pdfStreams);
}

