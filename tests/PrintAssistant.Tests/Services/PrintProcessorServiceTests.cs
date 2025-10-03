using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PrintAssistant.Configuration;
using PrintAssistant.Core;
using PrintAssistant.Services;
using PrintAssistant.Services.Abstractions;
using Xunit;

namespace PrintAssistant.Tests.Services;

public class PrintProcessorServiceTests
{
    private static PrintProcessorService CreateService(
        Mock<IPrintQueue> queueMock,
        Mock<IPrintService> printServiceMock,
        Mock<IFileMonitor> fileMonitorMock,
        Mock<ITrayIconService> trayIconMock,
        Mock<IFileConverterFactory> converterFactoryMock,
        Mock<IPdfMerger> pdfMergerMock,
        Mock<IFileArchiver> archiverMock,
        Mock<ICoverPageGenerator> coverPageGeneratorMock,
        IRetryPolicy retryPolicy,
        IJobStageRetryDecider retryDecider,
        AppSettings? settings = null)
    {
        var options = Options.Create(settings ?? new AppSettings());
        var serviceProvider = new Mock<IServiceProvider>();

        return new PrintProcessorService(
            NullLogger<PrintProcessorService>.Instance,
            queueMock.Object,
            fileMonitorMock.Object,
            trayIconMock.Object,
            printServiceMock.Object,
            converterFactoryMock.Object,
            pdfMergerMock.Object,
            archiverMock.Object,
            coverPageGeneratorMock.Object,
            retryPolicy,
            retryDecider,
            options,
            serviceProvider.Object);
    }

    [Fact]
    public async Task ProcessJob_ShouldConvertMergePrintAndArchive()
    {
        // Arrange
        var queueMock = new Mock<IPrintQueue>();
        var printServiceMock = new Mock<IPrintService>();
        var fileMonitorMock = new Mock<IFileMonitor>();
        var trayIconMock = new Mock<ITrayIconService>();
        var converterFactoryMock = new Mock<IFileConverterFactory>();
        var pdfMergerMock = new Mock<IPdfMerger>();
        var archiverMock = new Mock<IFileArchiver>();
        var coverPageGeneratorMock = new Mock<ICoverPageGenerator>();

        printServiceMock.Setup(p => p.PrintPdfAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.CompletedTask);

        var job = new PrintJob(new[] { "file1.docx", "file2.pdf" })
        {
            SelectedPrinter = "PrinterA",
            Copies = 2
        };

        var settings = new AppSettings
        {
            Printing = new PrintSettings
            {
                GenerateCoverPage = true
            }
        };

        queueMock.SetupSequence(q => q.DequeueJobAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(job)
            .ThrowsAsync(new OperationCanceledException());

        // Mock converters
        converterFactoryMock.Setup(f => f.GetConverter("file1.docx"))
            .Returns(Mock.Of<IFileConverter>(c => c.ConvertToPdfAsync("file1.docx") == Task.FromResult<Stream>(new MemoryStream(new byte[] { 1, 2, 3 }))));
        converterFactoryMock.Setup(f => f.GetConverter("file2.pdf"))
            .Returns(Mock.Of<IFileConverter>(c => c.ConvertToPdfAsync("file2.pdf") == Task.FromResult<Stream>(new MemoryStream(new byte[] { 4, 5, 6 }))));

        // Mock cover page
        var coverStream = new MemoryStream(new byte[] { 7, 8, 9 });
        coverPageGeneratorMock.Setup(c => c.GenerateCoverPageAsync(It.IsAny<PrintJob>()))
            .ReturnsAsync(coverStream);

        // Mock merger
        var mergedStream = new MemoryStream(new byte[] { 10, 11 });
        pdfMergerMock.Setup(m => m.MergePdfsAsync(It.IsAny<IEnumerable<Func<Task<Stream>>>>()))
            .ReturnsAsync((mergedStream, 5));

        archiverMock.Setup(a => a.ArchiveFilesAsync(It.IsAny<IEnumerable<string>>(), job.CreationTime, It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("archive-path");

        var retryPolicyMock = new Mock<IRetryPolicy>();
        retryPolicyMock.Setup(p => p.GetDelay(It.IsAny<int>())).Returns((TimeSpan?)null);
        var retryDeciderMock = new Mock<IJobStageRetryDecider>();
        retryDeciderMock.Setup(d => d.ShouldRetry(It.IsAny<PrintJobStage>())).Returns(false);

        var service = CreateService(queueMock, printServiceMock, fileMonitorMock, trayIconMock, converterFactoryMock, pdfMergerMock, archiverMock, coverPageGeneratorMock, retryPolicyMock.Object, retryDeciderMock.Object, settings);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1000));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(300);
        await service.StopAsync(cts.Token);

        // Assert
        printServiceMock.Verify(p => p.PrintPdfAsync(mergedStream, "PrinterA", 2), Times.Once);
        pdfMergerMock.Verify(m => m.MergePdfsAsync(It.Is<IEnumerable<Func<Task<Stream>>>>(factories => factories.Count() == 3)), Times.Once);
        archiverMock.Verify(a => a.ArchiveFilesAsync(job.SourceFilePaths, job.CreationTime, It.IsAny<Stream?>(), It.Is<string>(name => name.EndsWith(".pdf"))), Times.Once);
        Assert.Equal(JobStatus.Archived, job.Status);
        Assert.Equal(5, job.PageCount);
    }

    [Fact]
    public async Task ProcessJob_ShouldHandleUnsupportedFiles()
    {
        var queueMock = new Mock<IPrintQueue>();
        var printServiceMock = new Mock<IPrintService>();
        var fileMonitorMock = new Mock<IFileMonitor>();
        var trayIconMock = new Mock<ITrayIconService>();
        var converterFactoryMock = new Mock<IFileConverterFactory>();
        var pdfMergerMock = new Mock<IPdfMerger>();
        var archiverMock = new Mock<IFileArchiver>();
        var coverPageGeneratorMock = new Mock<ICoverPageGenerator>();

        var job = new PrintJob(new[] { "unsupported.xyz" });
        queueMock.SetupSequence(q => q.DequeueJobAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(job)
            .ThrowsAsync(new OperationCanceledException());

        converterFactoryMock.Setup(f => f.GetConverter("unsupported.xyz"))
            .Returns((IFileConverter?)null);

        var retryPolicyMock = new Mock<IRetryPolicy>();
        retryPolicyMock.Setup(p => p.GetDelay(It.IsAny<int>())).Returns((TimeSpan?)null);
        var retryDeciderMock = new Mock<IJobStageRetryDecider>();
        retryDeciderMock.Setup(d => d.ShouldRetry(It.IsAny<PrintJobStage>())).Returns(false);

        var service = CreateService(queueMock, printServiceMock, fileMonitorMock, trayIconMock, converterFactoryMock, pdfMergerMock, archiverMock, coverPageGeneratorMock, retryPolicyMock.Object, retryDeciderMock.Object);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1000));

        await service.StartAsync(cts.Token);
        await Task.Delay(300);
        await service.StopAsync(cts.Token);

        archiverMock.Verify(a => a.MoveUnsupportedFile("unsupported.xyz"), Times.Once);
    }
}

