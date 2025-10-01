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

        var archivePath = await archiver.ArchiveFilesAsync(new[] { sourceFilePath }, jobTime);

        var expectedArchivePath = @"C:\PrintJobs\Processed_20231027\test.txt";
        Assert.Equal(@"C:\PrintJobs\Processed_20231027", archivePath);
        Assert.False(mockFileSystem.FileExists(sourceFilePath));
        Assert.True(mockFileSystem.FileExists(expectedArchivePath));
    }
    
    [Fact]
    public async Task ArchiveFilesAsync_ShouldPersistMergedPdf()
    {
        var mockFileSystem = new MockFileSystem();
        var monitorPath = @"C:\PrintJobs";
        var sourceFilePath = @"C:\PrintJobs\test2.txt";
        mockFileSystem.AddFile(sourceFilePath, new MockFileData("test2 content"));

        var appSettings = new AppSettings
        {
            Monitoring = new MonitorSettings { Path = monitorPath },
            Archiving = new ArchiveSettings { SubdirectoryFormat = "Processed_{0:yyyyMMdd}" }
        };
        var archiver = new FileArchiver(mockFileSystem, Options.Create(appSettings));
        var jobTime = new DateTime(2024, 1, 15);

        await using var mergedStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var archivePath = await archiver.ArchiveFilesAsync(new[] { sourceFilePath }, jobTime, mergedStream, "merged.pdf");

        var expectedMergedPath = @"C:\PrintJobs\Processed_20240115\merged.pdf";
        Assert.Equal(@"C:\PrintJobs\Processed_20240115", archivePath);
        Assert.True(mockFileSystem.FileExists(expectedMergedPath));
        Assert.False(mockFileSystem.FileExists(sourceFilePath));
    }
}

