using Moq;
using PrintAssistant.Services;
using PrintAssistant.Services.Abstractions;
using PrintAssistant.Services.Converters;
using System.Text;
using Xunit;

namespace PrintAssistant.Tests.Services;

public class FileConverterFactoryTests
{
    private FileConverterFactory CreateFactory()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(WordToPdfConverter)))
            .Returns(Mock.Of<IFileConverter>());
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ExcelToPdfConverter)))
            .Returns(Mock.Of<IFileConverter>());
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ImageToPdfConverter)))
            .Returns(Mock.Of<IFileConverter>());

        return new FileConverterFactory(serviceProviderMock.Object);
    }

    [Theory]
    [InlineData("sample.docx")]
    [InlineData("document.DOC")]
    public void GetConverter_ShouldReturnWordConverter(string fileName)
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter(fileName);
        Assert.NotNull(converter);
    }

    [Theory]
    [InlineData("sheet.xls")]
    [InlineData("sheet.xlsx")]
    [InlineData("sheet.XLSM")]
    public void GetConverter_ShouldReturnExcelConverter(string fileName)
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter(fileName);
        Assert.NotNull(converter);
    }

    [Theory]
    [InlineData("image.jpg")]
    [InlineData("image.PNG")]
    [InlineData("image.bmp")]
    public void GetConverter_ShouldReturnImageConverter(string fileName)
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter(fileName);
        Assert.NotNull(converter);
    }

    [Fact]
    public async Task GetConverter_ShouldReturnPassthroughForPdf()
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter("already.pdf");
        Assert.NotNull(converter);

        var tempFile = Path.Combine(Path.GetTempPath(), $"PrintAssistantTest_{Guid.NewGuid():N}.pdf");
        var pdfBytes = Encoding.UTF8.GetBytes("%PDF-1.4\n1 0 obj\n<< /Type /Catalog >>\nendobj\ntrailer\n<< /Root 1 0 R >>\nstartxref\n0\n%%EOF");
        await File.WriteAllBytesAsync(tempFile, pdfBytes);

        try
        {
            await using var resultStream = await converter!.ConvertToPdfAsync(tempFile);
            Assert.True(resultStream.Length > 0);

            resultStream.Position = 0;
            using var memory = new MemoryStream();
            await resultStream.CopyToAsync(memory);
            Assert.Equal(pdfBytes, memory.ToArray());
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task PassthroughConverter_ShouldHandleLargePdfFile()
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter("large.pdf");
        Assert.NotNull(converter);

        var tempFile = Path.Combine(Path.GetTempPath(), $"PrintAssistantTest_{Guid.NewGuid():N}_large.pdf");

        await using (var fileStream = File.Create(tempFile))
        {
            var header = Encoding.ASCII.GetBytes("%PDF-1.4\n");
            await fileStream.WriteAsync(header);

            var content = new byte[5 * 1024 * 1024];
            Random.Shared.NextBytes(content);
            await fileStream.WriteAsync(content);

            var trailer = Encoding.ASCII.GetBytes("\ntrailer\n<< /Root 1 0 R >>\nstartxref\n0\n%%EOF");
            await fileStream.WriteAsync(trailer);
        }

        try
        {
            await using var resultStream = await converter!.ConvertToPdfAsync(tempFile);
            Assert.Equal(new FileInfo(tempFile).Length, resultStream.Length);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task PassthroughConverter_ShouldThrowForMissingFile()
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter("missing.pdf");
        Assert.NotNull(converter);

        var missingPath = Path.Combine(Path.GetTempPath(), $"PrintAssistantTest_{Guid.NewGuid():N}_missing.pdf");

        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => converter!.ConvertToPdfAsync(missingPath));
        Assert.Contains(Path.GetFileName(missingPath), exception.FileName ?? string.Empty);
    }

    [Fact]
    public async Task PassthroughConverter_ShouldThrowForInvalidPdf()
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter("invalid.pdf");
        Assert.NotNull(converter);

        var tempFile = Path.Combine(Path.GetTempPath(), $"PrintAssistantTest_{Guid.NewGuid():N}_invalid.pdf");
        await File.WriteAllTextAsync(tempFile, "Not a PDF file");

        try
        {
            await using var stream = await converter!.ConvertToPdfAsync(tempFile);
            Assert.True(stream.Length > 0);

            stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            var content = await reader.ReadToEndAsync();
            Assert.Equal("Not a PDF file", content);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void GetConverter_ShouldReturnNullForUnsupported()
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter("unsupported.xyz");
        Assert.Null(converter);
    }
}


