using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;

namespace PrintAssistant.Services.Retry;

public class DefaultRetryPolicy : IRetryPolicy, IJobStageRetryDecider
{
    private readonly PrintRetryPolicySettings _settings;
    private readonly HashSet<PrintJobStage> _retryStages;

    public DefaultRetryPolicy(IOptions<AppSettings> options)
    {
        _settings = options.Value.Printing.RetryPolicy;
        _retryStages = _settings.RetryOn
            .Select(name => Enum.TryParse<PrintJobStage>(name, ignoreCase: true, out var stage) ? stage : (PrintJobStage?)null)
            .Where(stage => stage.HasValue)
            .Select(stage => stage!.Value)
            .ToHashSet();
    }

    public TimeSpan? GetDelay(int attempt)
    {
        if (attempt >= _settings.MaxRetryCount)
        {
            return null;
        }

        var delay = _settings.InitialDelayMilliseconds * Math.Pow(_settings.BackoffFactor, attempt);
        delay = Math.Min(delay, _settings.MaxDelayMilliseconds);
        return TimeSpan.FromMilliseconds(delay);
    }

    public bool ShouldRetry(PrintJobStage stage)
    {
        return _retryStages.Contains(stage);
    }
}

