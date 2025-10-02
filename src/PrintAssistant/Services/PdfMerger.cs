using PrintAssistant.Services.Abstractions;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;

namespace PrintAssistant.Services;

public class PdfMerger : IPdfMerger
{
    public async Task<(Stream MergedPdfStream, int TotalPages)> MergePdfsAsync(IEnumerable<Func<Task<Stream>>> pdfFactories)
    {
        if (pdfFactories == null)
        {
            throw new ArgumentNullException(nameof(pdfFactories));
        }

        using var mergedDocument = new PdfDocument();
        var totalPages = 0;
        var openedStreams = new List<Stream>();
        var loadedDocuments = new List<PdfLoadedDocument>();

        try
        {
            foreach (var factory in pdfFactories)
            {
                if (factory == null)
                {
                    continue;
                }

                var pdfStream = await factory().ConfigureAwait(false);
                if (pdfStream == null)
                {
                    continue;
                }

                if (pdfStream.CanSeek)
                {
                    pdfStream.Position = 0;
                }

                var loadedDocument = new PdfLoadedDocument(pdfStream);
                mergedDocument.Append(loadedDocument);
                totalPages += loadedDocument.Pages.Count;

                openedStreams.Add(pdfStream);
                loadedDocuments.Add(loadedDocument);
            }

            if (totalPages == 0)
            {
                return (Stream.Null, 0);
            }

            var resultStream = new MemoryStream();
            mergedDocument.Save(resultStream);
            await resultStream.FlushAsync().ConfigureAwait(false);
            resultStream.Position = 0;

            return (resultStream, totalPages);
        }
        finally
        {
            foreach (var document in loadedDocuments)
            {
                document.Close(true);
            }

            foreach (var stream in openedStreams)
            {
                stream.Dispose();
            }
        }
    }
}

