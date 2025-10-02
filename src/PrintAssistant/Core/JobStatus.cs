namespace PrintAssistant.Core;

public enum JobStatus
{
    Pending,
    Processing,
    Converting,
    Retrying,
    Printing,
    Completed,
    Failed,
    Cancelled,
    Archived
}

