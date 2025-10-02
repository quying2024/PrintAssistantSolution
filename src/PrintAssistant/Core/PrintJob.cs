namespace PrintAssistant.Core;

public class PrintJob
{
    public Guid JobId { get; }
    public DateTime CreationTime { get; }
    public IReadOnlyList<string> SourceFilePaths { get; }
    public JobStatus Status { get; set; }
    public int PageCount { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SelectedPrinter { get; set; }
    public int Copies { get; set; }
    public PrintJobStage? LastFailedStage { get; set; }
    public int AttemptCount { get; set; }
    public int MaxRetryCount { get; set; }

    public PrintJob(IEnumerable<string> sourceFilePaths)
    {
        JobId = Guid.NewGuid();
        CreationTime = DateTime.UtcNow;
        SourceFilePaths = sourceFilePaths.ToList().AsReadOnly();
        Status = JobStatus.Pending;
        Copies = 1;
    }
}

