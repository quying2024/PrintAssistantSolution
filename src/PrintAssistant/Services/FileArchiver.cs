using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using PrintAssistant.Services.Abstractions;
using System.IO.Abstractions;

namespace PrintAssistant.Services;

/// <summary>
/// 负责将已处理文件归档并处理不支持的文件类型。
/// </summary>
public class FileArchiver : IFileArchiver
{
    private readonly IFileSystem _fileSystem;
    private readonly AppSettings _settings;

    public FileArchiver(IFileSystem fileSystem, IOptions<AppSettings> appSettings)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _settings = appSettings?.Value ?? throw new ArgumentNullException(nameof(appSettings));
    }

    public Task ArchiveFilesAsync(IEnumerable<string> sourceFiles, DateTime jobCreationTime)
    {
        if (sourceFiles == null)
        {
            throw new ArgumentNullException(nameof(sourceFiles));
        }

        var monitorPath = GetMonitorPath();
        var archiveSubDirName = string.Format(_settings.Archiving.SubdirectoryFormat, jobCreationTime);
        var archivePath = _fileSystem.Path.Combine(monitorPath, archiveSubDirName);

        _fileSystem.Directory.CreateDirectory(archivePath);

        foreach (var sourceFile in sourceFiles)
        {
            if (sourceFile is null)
            {
                continue;
            }

            if (_fileSystem.File.Exists(sourceFile))
            {
                var destFileName = _fileSystem.Path.Combine(archivePath, _fileSystem.Path.GetFileName(sourceFile));
                _fileSystem.File.Move(sourceFile, destFileName);
            }
        }

        return Task.CompletedTask;
    }

    public void MoveUnsupportedFile(string sourceFile)
    {
        if (string.IsNullOrWhiteSpace(sourceFile))
        {
            throw new ArgumentException("Source file path cannot be null or whitespace.", nameof(sourceFile));
        }

        var unsupportedPath = GetUnsupportedFilesPath();
        _fileSystem.Directory.CreateDirectory(unsupportedPath);

        if (_fileSystem.File.Exists(sourceFile))
        {
            var destFileName = _fileSystem.Path.Combine(unsupportedPath, _fileSystem.Path.GetFileName(sourceFile));
            _fileSystem.File.Move(sourceFile, destFileName);
        }
    }

    private string GetMonitorPath()
    {
        return !string.IsNullOrEmpty(_settings.Monitoring.Path)
            ? _settings.Monitoring.Path
            : _fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PrintJobs");
    }

    private string GetUnsupportedFilesPath()
    {
        return !string.IsNullOrEmpty(_settings.UnsupportedFiles.MoveToPath)
            ? _settings.UnsupportedFiles.MoveToPath
            : _fileSystem.Path.Combine(GetMonitorPath(), "Unsupported");
    }
}

