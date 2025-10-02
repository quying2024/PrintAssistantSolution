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
    public bool UseMockPrintService { get; set; } = true;
    public PrintRetryPolicySettings RetryPolicy { get; set; } = new();
    public WindowsPrintSettings Windows { get; set; } = new();
}

public class ArchiveSettings
{
    public string SubdirectoryFormat { get; set; } = "Processed_{0:yyyyMMdd_HHmmss}";
}

public class LogSettings
{
    public string? Path { get; set; }
    public int RetentionDays { get; set; } = 7;
}

public class PrintRetryPolicySettings
{
    public int MaxRetryCount { get; set; } = 3;
    public int InitialDelayMilliseconds { get; set; } = 1000;
    public double BackoffFactor { get; set; } = 2d;
    public int MaxDelayMilliseconds { get; set; } = 30000;
    public List<string> RetryOn { get; set; } = new() { "Conversion", "Merge", "Print", "Archive" };
}

public class WindowsPrintSettings
{
    public string? DefaultPrinter { get; set; }
    public int DefaultCopies { get; set; } = 1;
    public bool Duplex { get; set; }
    public bool Collate { get; set; } = true;
    public string? PaperSource { get; set; }
    public bool Landscape { get; set; }
    public bool StretchToFit { get; set; } = true;
    public bool Color { get; set; } = true;
    public int? Dpi { get; set; }
}

