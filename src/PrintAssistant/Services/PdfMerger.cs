using PrintAssistant.Services.Abstractions;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;

namespace PrintAssistant.Services;

public class PdfMerger : IPdfMerger
{
    public async Task<(Stream MergedPdfStream, int TotalPages)> MergePdfsAsync(IEnumerable<Stream> pdfStreams)
    {
        if (pdfStreams == null)
        {
            throw new ArgumentNullException(nameof(pdfStreams));
        }

        var streams = pdfStreams.Where(stream => stream != null).ToList();
        if (streams.Count == 0)
        {
            return (Stream.Null, 0);
        }

        using var mergedDocument = new PdfDocument();

        foreach (var stream in streams)
        {
            stream.Position = 0;
            using var loadedDocument = new PdfLoadedDocument(stream);
            mergedDocument.Append(loadedDocument);
        }

        var resultStream = new MemoryStream();
        mergedDocument.Save(resultStream);
        await resultStream.FlushAsync().ConfigureAwait(false);
        resultStream.Position = 0;

        var totalPages = mergedDocument.Pages.Count;
        mergedDocument.Close(true);

        return (resultStream, totalPages);
    }
}

