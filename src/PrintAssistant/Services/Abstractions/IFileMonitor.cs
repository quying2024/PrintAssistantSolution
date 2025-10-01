using PrintAssistant.Core;

namespace PrintAssistant.Services.Abstractions;

public interface IFileMonitor
{
    event Action<PrintJob>? JobDetected;
    void StartMonitoring();
    void StopMonitoring();
}

