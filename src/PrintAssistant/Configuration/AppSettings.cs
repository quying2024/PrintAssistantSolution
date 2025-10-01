namespace PrintAssistant.Configuration;

/// <summary>
/// 应用程序设置的根对象，用于绑定 appsettings.json 中的 "AppSettings" 部分。
/// </summary>
public class AppSettings
{
    public MonitorSettings Monitoring { get; set; } = new();

    public UnsupportedFileSettings UnsupportedFiles { get; set; } = new();

    public PrintSettings Printing { get; set; } = new();

    public ArchiveSettings Archiving { get; set; } = new();

    public LogSettings Logging { get; set; } = new();
}

