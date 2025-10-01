namespace PrintAssistant.Services.Abstractions;

/// <summary>
/// 文件归档服务接口。
/// </summary>
public interface IFileArchiver
{
    /// <summary>
    /// 将任务文件归档至时间戳目录。
    /// </summary>
    Task ArchiveFilesAsync(IEnumerable<string> sourceFiles, DateTime jobCreationTime);

    /// <summary>
    /// 将不支持的文件移动到预设目录。
    /// </summary>
    void MoveUnsupportedFile(string sourceFile);
}

