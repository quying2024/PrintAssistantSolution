using PrintAssistant.Services.Abstractions;

namespace PrintAssistant.Services;

public class PdfMerger : IPdfMerger
{
    public Task<(Stream MergedPdfStream, int TotalPages)> MergePdfsAsync(IEnumerable<Stream> pdfStreams)
    {
        // TODO: Implement PDF merge logic in subsequent iterations.
        return Task.FromResult<(Stream, int)>((Stream.Null, 0));
    }
}

