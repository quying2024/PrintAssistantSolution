namespace PrintAssistant.Configuration;

public class AppSettings
{
    public MonitorSettings Monitoring { get; set; } = new();
    public UnsupportedFileSettings UnsupportedFiles { get; set; } = new();
    public PrintSettings Printing { get; set; } = new();
    public ArchiveSettings Archiving { get; set; } = new();
    public LogSettings Logging { get; set; } = new();
}

