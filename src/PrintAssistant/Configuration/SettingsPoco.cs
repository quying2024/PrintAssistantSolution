namespace PrintAssistant.Configuration;

public class MonitorSettings
{
    public string? Path { get; set; }
    public int DebounceIntervalMilliseconds { get; set; } = 2500;
    public long MaxFileSizeMegaBytes { get; set; } = 100;
}

public class UnsupportedFileSettings
{
    public string? MoveToPath { get; set; }
}

public class PrintSettings
{
    public List<string> ExcludedPrinters { get; set; } = new();
    public bool GenerateCoverPage { get; set; } = true;
}

public class ArchiveSettings
{
    public string SubdirectoryFormat { get; set; } = "Processed_{0:yyyyMMdd_HHmmss}";
}

public class LogSettings
{
    public string? Path { get; set; }
}

