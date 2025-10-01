namespace PrintAssistant.Services.Abstractions;

/// <summary>
/// 文件归档服务接口。
/// </summary>
public interface IFileArchiver
{
    /// <summary>
    /// 将任务文件及可选的合并输出归档至时间戳目录，返回最终归档目录路径。
    /// </summary>
    Task<string> ArchiveFilesAsync(IEnumerable<string> sourceFiles, DateTime jobCreationTime, Stream? mergedPdfStream = null, string? mergedFileName = null);

    /// <summary>
    /// 将不支持的文件移动到预设目录。
    /// </summary>
    void MoveUnsupportedFile(string sourceFile);
}

