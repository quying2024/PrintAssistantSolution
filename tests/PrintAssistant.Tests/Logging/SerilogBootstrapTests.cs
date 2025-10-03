using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Xunit;

namespace PrintAssistant.Tests.Logging;

public class SerilogBootstrapTests
{
    [Fact]
    public void Bootstrap_ShouldCreateLogsDirectoryAndFile()
    {
        // Arrange
        var baseDirectory = Path.Combine(Path.GetTempPath(), "PrintAssistantTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(baseDirectory);
        var originalDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = baseDirectory;

        var configurationJson = "{\"Serilog\":{\"WriteTo\":[{\"Name\":\"File\",\"Args\":{\"path\":\"Logs\\\\log-.txt\",\"rollingInterval\":\"Day\"}}]}}";
        var tempConfigPath = Path.Combine(baseDirectory, "appsettings.json");
        File.WriteAllText(tempConfigPath, configurationJson);

        var configuration = new ConfigurationManager();
        configuration.AddJsonFile(tempConfigPath, optional: false, reloadOnChange: false);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        Log.Information("Bootstrap test log entry.");
        Log.CloseAndFlush();

        // Assert
        var logsPath = Path.Combine(baseDirectory, "Logs");
        Assert.True(Directory.Exists(logsPath), "Logs directory should be created.");
        var files = Directory.GetFiles(logsPath, "log-*.txt");
        Assert.NotEmpty(files);
        Assert.Contains(files, file => new FileInfo(file).Length > 0);

        // Cleanup
        Environment.CurrentDirectory = originalDirectory;
        Directory.Delete(baseDirectory, recursive: true);
    }
}
