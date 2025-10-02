namespace PrintAssistant.Services.Abstractions;

public interface IRetryPolicy
{
    TimeSpan? GetDelay(int attempt);
}
