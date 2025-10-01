using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using PrintAssistant.Services.Abstractions;
using System.IO.Abstractions;

namespace PrintAssistant.Services;

public class FileArchiver(IFileSystem fileSystem, IOptions<AppSettings> appSettings) : IFileArchiver
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly AppSettings _settings = appSettings.Value;

    public async Task<string> ArchiveFilesAsync(IEnumerable<string> sourceFiles, DateTime jobCreationTime, Stream? mergedPdfStream = null, string? mergedFileName = null)
    {
        var monitorPath = GetMonitorPath();
        var archiveSubDirName = string.Format(_settings.Archiving.SubdirectoryFormat, jobCreationTime);
        var archivePath = _fileSystem.Path.Combine(monitorPath, archiveSubDirName);

        _fileSystem.Directory.CreateDirectory(archivePath);

        foreach (var sourceFile in sourceFiles)
        {
            if (_fileSystem.File.Exists(sourceFile))
            {
                var destFileName = _fileSystem.Path.Combine(archivePath, _fileSystem.Path.GetFileName(sourceFile));
                if (_fileSystem.File.Exists(destFileName))
                {
                    _fileSystem.File.Delete(destFileName);
                }
                _fileSystem.File.Move(sourceFile, destFileName);
            }
        }

        if (mergedPdfStream != null)
        {
            mergedPdfStream.Position = 0;
            var targetName = string.IsNullOrWhiteSpace(mergedFileName)
                ? $"Merged_{jobCreationTime:yyyyMMdd_HHmmss}.pdf"
                : mergedFileName!;
            var mergedPath = _fileSystem.Path.Combine(archivePath, targetName);
            await using var fileStream = _fileSystem.File.Create(mergedPath);
            await mergedPdfStream.CopyToAsync(fileStream);
        }

        return archivePath;
    }

    public void MoveUnsupportedFile(string sourceFile)
    {
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

