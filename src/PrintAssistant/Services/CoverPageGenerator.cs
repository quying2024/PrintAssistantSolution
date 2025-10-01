using System.Globalization;
using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;

namespace PrintAssistant.Services;

public class CoverPageGenerator : ICoverPageGenerator
{
    public Task<Stream> GenerateCoverPageAsync(PrintJob job)
    {
        if (job == null)
        {
            throw new ArgumentNullException(nameof(job));
        }

        var document = new PdfDocument();
        var page = document.Pages.Add();
        var graphics = page.Graphics;

        var titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 20, PdfFontStyle.Bold);
        var bodyFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12);

        var culture = CultureInfo.CurrentCulture;

        float currentY = 40;
        graphics.DrawString("打印任务封面", titleFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(40, currentY));
        currentY += 40;

        graphics.DrawString($"任务编号: {job.JobId}", bodyFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(40, currentY));
        currentY += 25;

        graphics.DrawString($"创建时间: {job.CreationTime.ToString("f", culture)}", bodyFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(40, currentY));
        currentY += 25;

        graphics.DrawString($"文件数量: {job.SourceFilePaths.Count}", bodyFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(40, currentY));
        currentY += 25;

        var filesLabel = "文件列表:";
        graphics.DrawString(filesLabel, bodyFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(40, currentY));
        currentY += 20;

        foreach (var file in job.SourceFilePaths)
        {
            var name = Path.GetFileName(file);
            graphics.DrawString($"- {name}", bodyFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(60, currentY));
            currentY += 18;
        }

        var stream = new MemoryStream();
        document.Save(stream);
        stream.Position = 0;
        document.Close(true);

        return Task.FromResult<Stream>(stream);
    }
}


