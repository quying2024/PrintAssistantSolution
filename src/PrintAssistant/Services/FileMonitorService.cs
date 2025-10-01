using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;

namespace PrintAssistant.Services;

public class FileMonitorService : IFileMonitor, IDisposable
{
    public event Action<PrintJob>? JobDetected;

    public void StartMonitoring()
    {
        // TODO: Implement file monitoring logic in subsequent iterations.
    }

    public void StopMonitoring()
    {
        // TODO: Release resources when monitoring stops.
    }

    public void Dispose() => StopMonitoring();
}

