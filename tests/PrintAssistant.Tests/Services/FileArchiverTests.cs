using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using PrintAssistant.Services;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace PrintAssistant.Tests.Services;

public class FileArchiverTests
{
    [Fact]
    public async Task ArchiveFilesAsync_ShouldMoveFilesToTimestampedDirectory()
    {
        var mockFileSystem = new MockFileSystem();
        var monitorPath = @"C:\PrintJobs";
        var sourceFilePath = @"C:\PrintJobs\test.txt";
        mockFileSystem.AddFile(sourceFilePath, new MockFileData("test content"));

        var appSettings = new AppSettings
        {
            Monitoring = new MonitorSettings { Path = monitorPath },
            Archiving = new ArchiveSettings { SubdirectoryFormat = "Processed_{0:yyyyMMdd}" }
        };
        var mockOptions = Options.Create(appSettings);

        var archiver = new FileArchiver(mockFileSystem, mockOptions);
        var jobTime = new DateTime(2023, 10, 27);

        await archiver.ArchiveFilesAsync(new[] { sourceFilePath }, jobTime);

        var expectedArchivePath = @"C:\PrintJobs\Processed_20231027\test.txt";
        Assert.False(mockFileSystem.FileExists(sourceFilePath));
        Assert.True(mockFileSystem.FileExists(expectedArchivePath));
    }
}

