using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using PrintAssistant.Core;
using PrintAssistant.Services;
using System.IO.Abstractions;
using Xunit;

namespace PrintAssistant.Tests.Services;

public sealed class FileMonitorServiceTests : IDisposable
{
    private readonly string _tempDirectory;

    public FileMonitorServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public async Task FlushPendingFiles_ShouldRaiseJobDetected_WithDistinctFiles()
    {
        // Arrange
        var appSettings = Options.Create(new AppSettings
        {
            Monitoring = new MonitorSettings
            {
                Path = _tempDirectory,
                DebounceIntervalMilliseconds = 50,
                MaxFileSizeMegaBytes = 100
            }
        });

        using var service = new FileMonitorService(appSettings, new FileSystem(), NullLogger<FileMonitorService>.Instance);

        var tcs = new TaskCompletionSource<PrintJob>(TaskCreationOptions.RunContinuationsAsynchronously);
        service.JobDetected += job => tcs.TrySetResult(job);

        // Set monitor path directly to bypass watcher initialisation.
        typeof(FileMonitorService)
            .GetField("_monitorPath", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(service, _tempDirectory);

        var handleMethod = typeof(FileMonitorService)
            .GetMethod("HandleFileEvent", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;
        var flushMethod = typeof(FileMonitorService)
            .GetMethod("FlushPendingFiles", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;

        var fileA = Path.Combine(_tempDirectory, "a.txt");
        var fileB = Path.Combine(_tempDirectory, "b.txt");
        await File.WriteAllTextAsync(fileA, "hello");
        await File.WriteAllTextAsync(fileB, "world");

        // Act - simulate mixed events
        handleMethod.Invoke(service, new object?[] { fileA });
        handleMethod.Invoke(service, new object?[] { fileA });
        handleMethod.Invoke(service, new object?[] { fileB });

        flushMethod.Invoke(service, Array.Empty<object>());

        var job = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

        // Assert
        Assert.Equal(2, job.SourceFilePaths.Count);
        Assert.Contains(fileA, job.SourceFilePaths);
        Assert.Contains(fileB, job.SourceFilePaths);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
        catch
        {
            // ignore cleanup exceptions
        }
    }
}

