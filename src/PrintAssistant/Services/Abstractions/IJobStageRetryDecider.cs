using PrintAssistant.Core;

namespace PrintAssistant.Services.Abstractions;

public interface IJobStageRetryDecider
{
    bool ShouldRetry(PrintJobStage stage);
}

