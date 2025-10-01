using Moq;
using PrintAssistant.Services;
using PrintAssistant.Services.Abstractions;
using PrintAssistant.Services.Converters;
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
    }

    [Fact]
    public void GetConverter_ShouldReturnNullForUnsupported()
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter("unsupported.xyz");
        Assert.Null(converter);
    }
}


