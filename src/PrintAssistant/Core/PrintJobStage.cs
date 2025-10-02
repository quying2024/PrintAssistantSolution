namespace PrintAssistant.Core;

/// <summary>
/// 标识打印作业在处理流程中的阶段，用于异常处理与重试策略。
/// </summary>
public enum PrintJobStage
{
    Conversion,
    Merge,
    Print,
    Archive,
}

