using PrintAssistant.Configuration;
using PrintAssistant.Core;

namespace PrintAssistant.Services.Retry;

public sealed class RetryContext
{
    private readonly PrintRetryPolicySettings _settings;

    public RetryContext(PrintJob job, PrintRetryPolicySettings settings)
    {
        Job = job;
        _settings = settings;
        Job.MaxRetryCount = settings.MaxRetryCount;
    }

    public PrintJob Job { get; }

    public int Attempt => Job.AttemptCount;
    public int MaxRetries { get; private set; }

    public void Initialize(int maxRetries)
    {
        MaxRetries = maxRetries;
        Job.MaxRetryCount = maxRetries;
    }

    public bool CanRetry(int currentAttempt)
    {
        return currentAttempt < _settings.MaxRetryCount;
    }

    public void IncrementAttempt(PrintJobStage stage, string message)
    {
        Job.AttemptCount++;
        Job.LastFailedStage = stage;
        Job.ErrorMessage = message;
    }

    public void Reset()
    {
        Job.AttemptCount = 0;
        Job.LastFailedStage = null;
        Job.ErrorMessage = null;
    }
}

