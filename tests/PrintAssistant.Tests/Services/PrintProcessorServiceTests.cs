using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PrintAssistant.Core;
using PrintAssistant.Services;
using PrintAssistant.Services.Abstractions;
using Xunit;

namespace PrintAssistant.Tests.Services;

public class PrintProcessorServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldConsumeJobs_FromQueue()
    {
        // Arrange
        var printQueueMock = new Mock<IPrintQueue>();
        var printServiceMock = new Mock<IPrintService>();
        var fileMonitorMock = new Mock<IFileMonitor>();
        var trayIconMock = new Mock<ITrayIconService>();

        var services = new ServiceCollection();
        services.AddTransient<PrintAssistant.UI.SettingsForm>(_ => new PrintAssistant.UI.SettingsForm(Mock.Of<Microsoft.Extensions.Options.IOptions<PrintAssistant.Configuration.AppSettings>>(), new System.IO.Abstractions.FileSystem()));
        var provider = services.BuildServiceProvider();

        var job = new PrintJob(new[] { "demo.txt" });
        printQueueMock.SetupSequence(q => q.DequeueJobAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(job)
            .Returns(async () =>
            {
                await Task.Delay(Timeout.Infinite, It.IsAny<CancellationToken>());
                return job;
            });

        var service = new PrintProcessorService(
            NullLogger<PrintProcessorService>.Instance,
            printQueueMock.Object,
            fileMonitorMock.Object,
            trayIconMock.Object,
            printServiceMock.Object,
            provider);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100, cts.Token);
        await service.StopAsync(cts.Token);

        // Assert
        printServiceMock.Verify(p => p.PrintPdfAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }
}

