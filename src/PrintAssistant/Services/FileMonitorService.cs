using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;
using System.IO.Abstractions;

namespace PrintAssistant.Services
{
    public class FileMonitorService : IFileMonitor, IDisposable
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<FileMonitorService> _logger;
        private readonly MonitorSettings _settings;

        private readonly HashSet<string> _pendingFiles = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _syncRoot = new();

        private FileSystemWatcher? _watcher;
        private System.Timers.Timer? _debounceTimer;
        private string _monitorPath = string.Empty;
        private bool _disposed;

        public event Action<PrintJob>? JobDetected;

        public FileMonitorService(
            IOptions<AppSettings> appSettings,
            IFileSystem fileSystem,
            ILogger<FileMonitorService> logger)
        {
            if (appSettings == null) throw new ArgumentNullException(nameof(appSettings));

            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = appSettings.Value.Monitoring ?? new MonitorSettings();
        }

        public void StartMonitoring()
        {
            ThrowIfDisposed();

            if (_watcher != null)
            {
                return;
            }

            _monitorPath = DetermineMonitorPath();
            _fileSystem.Directory.CreateDirectory(_monitorPath);

            _debounceTimer = new System.Timers.Timer(_settings.DebounceIntervalMilliseconds)
            {
                AutoReset = false,
            };
            _debounceTimer.Elapsed += (_, _) => FlushPendingFiles();

            _watcher = CreateWatcher(_monitorPath);
            _watcher.Created += OnWatcherEvent;
            _watcher.Changed += OnWatcherEvent;
            _watcher.Renamed += OnWatcherRenamed;
            _watcher.Error += OnWatcherError;
            _watcher.EnableRaisingEvents = true;

            _logger.LogInformation("File monitor started at {MonitorPath}", _monitorPath);
        }

        public void StopMonitoring()
        {
            if (_watcher == null)
            {
                return;
            }

            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= OnWatcherEvent;
            _watcher.Changed -= OnWatcherEvent;
            _watcher.Renamed -= OnWatcherRenamed;
            _watcher.Error -= OnWatcherError;
            _watcher.Dispose();
            _watcher = null;

            if (_debounceTimer != null)
            {
                _debounceTimer.Stop();
                _debounceTimer.Dispose();
                _debounceTimer = null;
            }

            lock (_syncRoot)
            {
                _pendingFiles.Clear();
            }

            _logger.LogInformation("File monitor stopped for {MonitorPath}", _monitorPath);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            StopMonitoring();
            _disposed = true;
        }

        internal void HandleFileEvent(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            try
            {
                string fullPath = _fileSystem.Path.GetFullPath(filePath);

                if (!_fileSystem.File.Exists(fullPath))
                {
                    return;
                }

                if (!IsWithinMonitoredDirectory(fullPath))
                {
                    return;
                }

                if (ExceedsSizeLimit(fullPath))
                {
                    _logger.LogWarning("Ignoring file '{FilePath}' because it exceeds the size limit of {Limit} MB.", fullPath, _settings.MaxFileSizeMegaBytes);
                    return;
                }

                lock (_syncRoot)
                {
                    _pendingFiles.Add(fullPath);
                }

                RestartTimer();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while handling file event for {FilePath}", filePath);
            }
        }

        internal void FlushPendingFiles()
        {
            List<string> files;

            lock (_syncRoot)
            {
                if (_pendingFiles.Count == 0)
                {
                    return;
                }

                files = _pendingFiles.ToList();
                _pendingFiles.Clear();
            }

            files = files.Where(_fileSystem.File.Exists).ToList();
            if (files.Count == 0)
            {
                return;
            }

            var job = new PrintJob(files);
            JobDetected?.Invoke(job);

            _logger.LogInformation("Detected new print job {JobId} with {FileCount} files.", job.JobId, files.Count);
        }

        private void OnWatcherEvent(object sender, FileSystemEventArgs e) => HandleFileEvent(e.FullPath);

        private void OnWatcherRenamed(object sender, RenamedEventArgs e) => HandleFileEvent(e.FullPath);

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            _logger.LogError(e.GetException(), "FileSystemWatcher encountered an error. Attempting to restart monitoring.");
            StopMonitoring();
            StartMonitoring();
        }

        private bool ExceedsSizeLimit(string path)
        {
            if (_settings.MaxFileSizeMegaBytes <= 0)
            {
                return false;
            }

            long sizeLimitBytes = _settings.MaxFileSizeMegaBytes * 1024L * 1024L;
            var info = _fileSystem.FileInfo.New(path);
            return info.Exists && info.Length > sizeLimitBytes;
        }

        private bool IsWithinMonitoredDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(_monitorPath))
            {
                return false;
            }

            var normalizedMonitor = _fileSystem.Path.GetFullPath(_monitorPath).TrimEnd(_fileSystem.Path.DirectorySeparatorChar);
            var normalizedPath = _fileSystem.Path.GetFullPath(path);

            return normalizedPath.StartsWith(normalizedMonitor + _fileSystem.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(normalizedPath, normalizedMonitor, StringComparison.OrdinalIgnoreCase);
        }

        private void RestartTimer()
        {
            if (_debounceTimer == null)
            {
                return;
            }

            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private string DetermineMonitorPath()
        {
            if (!string.IsNullOrWhiteSpace(_settings.Path))
            {
                return _settings.Path;
            }

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            return _fileSystem.Path.Combine(desktop, "PrintJobs");
        }

        private FileSystemWatcher CreateWatcher(string path)
        {
            return new FileSystemWatcher(path)
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.*"
            };
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(FileMonitorService));
            }
        }
    }
}

