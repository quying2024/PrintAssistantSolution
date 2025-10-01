

### **项目目录结构**

PrintAssistantSolution/  
│  
├── src/  
│   └── PrintAssistant/  
│       ├── PrintAssistant.csproj  
│       ├── appsettings.json  
│       ├── Program.cs  
│       │  
│       ├── Assets/  
│       │   └── print\_icon.ico  
│       │  
│       ├── Configuration/  
│       │   ├── AppSettings.cs  
│       │   └── SettingsPoco.cs  
│       │  
│       ├── Core/  
│       │   ├── PrintJob.cs  
│       │   └── JobStatus.cs  
│       │  
│       ├── Services/  
│       │   ├── Abstractions/  
│       │   │   ├── ICoverPageGenerator.cs  
│       │   │   ├── IFileArchiver.cs  
│       │   │   ├── IFileConverter.cs  
│       │   │   ├── IFileConverterFactory.cs  
│       │   │   ├── IFileMonitor.cs  
│       │   │   ├── IPdfMerger.cs  
│       │   │   ├── IPrintQueue.cs  
│       │   │   ├── IPrintService.cs  
│       │   │   └── ITrayIconService.cs  
│       │   │  
│       │   ├── Converters/  
│       │   │   ├── ExcelToPdfConverter.cs  
│       │   │   ├── ImageToPdfConverter.cs  
│       │   │   └── WordToPdfConverter.cs  
│       │   │  
│       │   ├── CoverPageGenerator.cs  
│       │   ├── FileArchiver.cs  
│       │   ├── FileConverterFactory.cs  
│       │   ├── FileMonitorService.cs  
│       │   ├── PdfMerger.cs  
│       │   ├── PrintProcessorService.cs  
│       │   ├── PrintQueueService.cs  
│       │   ├── PrintService.cs  
│       │   └── TrayIconService.cs  
│       │  
│       └── UI/  
│           ├── PrinterSelectionForm.cs  
│           ├── PrinterSelectionForm.Designer.cs  
│           ├── SettingsForm.cs  
│           └── SettingsForm.Designer.cs  
│  
└── tests/  
    └── PrintAssistant.Tests/  
        ├── PrintAssistant.Tests.csproj  
        └── Services/  
            ├── FileArchiverTests.cs  
            └── FileMonitorServiceTests.cs

---

### **1\. 源代码 (src/PrintAssistant/)**

#### **PrintAssistant.csproj**

XML

\<Project Sdk\="Microsoft.NET.Sdk"\>

  \<PropertyGroup\>  
    \<OutputType\>WinExe\</OutputType\>  
    \<TargetFramework\>net8.0-windows\</TargetFramework\>  
    \<Nullable\>enable\</Nullable\>  
    \<UseWindowsForms\>true\</UseWindowsForms\>  
    \<ImplicitUsings\>enable\</ImplicitUsings\>  
    \<ApplicationIcon\>Assets\\print\_icon.ico\</ApplicationIcon\>  
  \</PropertyGroup\>

  \<ItemGroup\>  
    \<Content Include\="Assets\\print\_icon.ico" /\>  
  \</ItemGroup\>

  \<ItemGroup\>  
    \<PackageReference Include\="Microsoft.Extensions.Hosting" Version\="8.0.0" /\>  
    \<PackageReference Include\="Serilog.Extensions.Hosting" Version\="8.0.0" /\>  
    \<PackageReference Include\="Serilog.Settings.Configuration" Version\="8.0.0" /\>  
    \<PackageReference Include\="Serilog.Sinks.File" Version\="5.0.0" /\>  
    \<PackageReference Include\="System.IO.Abstractions" Version\="20.0.15" /\>  
    \<PackageReference Include\="System.Threading.Tasks.Dataflow" Version\="8.0.0" /\>  
    \<PackageReference Include\="Syncfusion.DocIO.Net.Core" Version\="\*" /\>  
    \<PackageReference Include\="Syncfusion.DocIORenderer.Net.Core" Version\="\*" /\>  
    \<PackageReference Include\="Syncfusion.Pdf.Net.Core" Version\="\*" /\>  
    \<PackageReference Include\="Syncfusion.PdfToImageConverter.Net" Version\="\*" /\>  
    \<PackageReference Include\="Syncfusion.XlsIO.Net.Core" Version\="\*" /\>  
    \<PackageReference Include\="Syncfusion.XlsIORenderer.Net.Core" Version\="\*" /\>  
  \</ItemGroup\>

  \<ItemGroup\>  
    \<None Update\="appsettings.json"\>  
      \<CopyToOutputDirectory\>PreserveNewest\</CopyToOutputDirectory\>  
    \</None\>  
  \</ItemGroup\>

\</Project\>

#### **appsettings.json**

JSON

{  
  "Serilog": {  
    "MinimumLevel": {  
      "Default": "Information",  
      "Override": {  
        "Microsoft": "Warning",  
        "System": "Warning"  
      }  
    },  
    "WriteTo": {Message:lj}{NewLine}{Exception}"  
        }  
      }  
    \]  
  },  
  "AppSettings": {  
    "Monitoring": {  
      "Path": "", // 留空则默认为桌面上的 "PrintJobs" 文件夹  
      "DebounceIntervalMilliseconds": 2500,  
      "MaxFileSizeMegaBytes": 100  
    },  
    "UnsupportedFiles": {  
      "MoveToPath": "" // 留空则默认为监控文件夹下的 "Unsupported" 子文件夹  
    },  
    "Printing": {  
      "ExcludedPrinters":,  
      "GenerateCoverPage": true  
    },  
    "Archiving": {  
      "SubdirectoryFormat": "Processed\_{0:yyyyMMdd\_HHmmss}"  
    },  
    "Logging": {  
      "Path": "" // 留空则默认为 C:\\Users\\用户名\\AppData\\Local\\PrintAssistant\\Logs  
    }  
  }  
}

#### **Program.cs**

```csharp
using System;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;  
using Microsoft.Extensions.Hosting;  
using PrintAssistant.Configuration;  
using PrintAssistant.Services;  
using PrintAssistant.Services.Abstractions;  
using Serilog;  
using System.IO.Abstractions;

namespace PrintAssistant;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.AddJsonFile("appsettings.Secret.json", optional: true, reloadOnChange: true);
        RegisterSyncfusionLicense(builder.Configuration);

        builder.Services.AddSerilog((services, loggerConfiguration) =>
            loggerConfiguration.ReadFrom.Configuration(builder.Configuration));

        ConfigureServices(builder.Services, builder.Configuration);

        using var host = builder.Build();

        _ = host.Services.GetRequiredService<ITrayIconService>();

        host.Start();

        Application.Run();

        host.StopAsync().GetAwaiter().GetResult();  
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)  
    {  
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IPrintQueue, PrintQueueService>();
        services.AddSingleton<IFileArchiver, FileArchiver>();
        services.AddSingleton<IFileMonitor, FileMonitorService>();
        services.AddSingleton<ITrayIconService, TrayIconService>();
        services.AddSingleton<IPrintService, MockPrintService>();
        services.AddSingleton<IPdfMerger, PdfMerger>();
        services.AddHostedService<PrintProcessorService>();
    }

    private static void RegisterSyncfusionLicense(IConfiguration configuration)
    {
        var licenseKeys = configuration.GetSection("Syncfusion:DocumentSdk:LicenseKeys").Get<string[]>();
        if (licenseKeys == null || licenseKeys.Length == 0)
        {
            return;
        }

        foreach (var key in licenseKeys)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(key);
            }
        }
    }
}
```

#### **Assets/print\_icon.ico**

这是一个图标文件。请您自行准备一个名为 print\_icon.ico 的图标文件并放置在此目录下。

#### **Configuration/AppSettings.cs**

C\#

namespace PrintAssistant.Configuration;

/// \<summary\>  
/// 应用程序设置的根对象，用于绑定 appsettings.json 中的 "AppSettings" 部分。  
/// \</summary\>  
public class AppSettings  
{  
    public MonitorSettings Monitoring { get; set; } \= new();  
    public UnsupportedFileSettings UnsupportedFiles { get; set; } \= new();  
    public PrintSettings Printing { get; set; } \= new();  
    public ArchiveSettings Archiving { get; set; } \= new();  
    public LogSettings Logging { get; set; } \= new();  
}

#### **Configuration/SettingsPoco.cs**

C\#

namespace PrintAssistant.Configuration;

// 以下是与 appsettings.json 中各个部分对应的强类型配置类 (POCO)。  
// 使用强类型配置可以提高代码的可读性、类型安全性和可维护性。

public class MonitorSettings  
{  
    public string? Path { get; set; }  
    public int DebounceIntervalMilliseconds { get; set; } \= 2500;  
    public long MaxFileSizeMegaBytes { get; set; } \= 100;  
}

public class UnsupportedFileSettings  
{  
    public string? MoveToPath { get; set; }  
}

public class PrintSettings  
{  
    public List\<string\> ExcludedPrinters { get; set; } \= new();  
    public bool GenerateCoverPage { get; set; } \= true;  
}

public class ArchiveSettings  
{  
    public string SubdirectoryFormat { get; set; } \= "Processed\_{0:yyyyMMdd\_HHmmss}";  
}

public class LogSettings  
{  
    public string? Path { get; set; }  
}

#### **Core/PrintJob.cs**

C\#

namespace PrintAssistant.Core;

/// \<summary\>  
/// 代表一个独立的打印任务。这是在整个处理管道中传递的核心数据对象 (DTO)。  
/// \</summary\>  
public class PrintJob  
{  
    /// \<summary\>  
    /// 任务的唯一标识符，用于日志追踪。  
    /// \</summary\>  
    public Guid JobId { get; }

    /// \<summary\>  
    /// 任务的创建时间。  
    /// \</summary\>  
    public DateTime CreationTime { get; }

    /// \<summary\>  
    /// 构成此任务的所有原始文件的路径列表。  
    /// \</summary\>  
    public IReadOnlyList\<string\> SourceFilePaths { get; }

    /// \<summary\>  
    /// 任务的当前状态。  
    /// \</summary\>  
    public JobStatus Status { get; set; }

    /// \<summary\>  
    /// 转换并合并后，最终PDF文档的总页数。  
    /// \</summary\>  
    public int PageCount { get; set; }

    /// \<summary\>  
    /// 如果任务失败，记录详细的错误信息。  
    /// \</summary\>  
    public string? ErrorMessage { get; set; }

    /// \<summary\>  
    /// 用户选择的目标打印机名称。  
    /// \</summary\>  
    public string? SelectedPrinter { get; set; }

    /// \<summary\>  
    /// 用户选择的打印份数。  
    /// \</summary\>  
    public int Copies { get; set; }

    public PrintJob(IEnumerable\<string\> sourceFilePaths)  
    {  
        JobId \= Guid.NewGuid();  
        CreationTime \= DateTime.UtcNow;  
        SourceFilePaths \= sourceFilePaths.ToList().AsReadOnly();  
        Status \= JobStatus.Pending;  
        Copies \= 1; // 默认份数  
    }  
}

#### **Core/JobStatus.cs**

C\#

namespace PrintAssistant.Core;

/// \<summary\>  
/// 定义打印任务在其生命周期中可能经历的各种状态。  
/// \</summary\>  
public enum JobStatus  
{  
    Pending,      // 待处理  
    Processing,   // 正在处理  
    Converting,   // 正在转换文件  
    Printing,     // 正在发送到打印机  
    Completed,    // 已成功完成  
    Failed,       // 处理失败  
    Cancelled,    // 用户取消  
    Archived      // 文件已归档  
}

#### **Services/Abstractions/ (所有接口)**

C\#

// ICoverPageGenerator.cs  
namespace PrintAssistant.Services.Abstractions;  
using PrintAssistant.Core;  
public interface ICoverPageGenerator { Task\<Stream\> GenerateCoverPageAsync(PrintJob job); }

// IFileArchiver.cs  
namespace PrintAssistant.Services.Abstractions;  
public interface IFileArchiver { Task<string> ArchiveFilesAsync(IEnumerable<string> sourceFiles, DateTime jobCreationTime, Stream? mergedPdfStream = null, string? mergedFileName = null); void MoveUnsupportedFile(string sourceFile); }

// IFileConverter.cs  
namespace PrintAssistant.Services.Abstractions;  
public interface IFileConverter { Task\<Stream\> ConvertToPdfAsync(string sourceFilePath); }

// IFileConverterFactory.cs  
namespace PrintAssistant.Services.Abstractions;  
public interface IFileConverterFactory { IFileConverter? GetConverter(string filePath); }

// IFileMonitor.cs  
namespace PrintAssistant.Services.Abstractions;  
using PrintAssistant.Core;  
public interface IFileMonitor { event Action\<PrintJob\>? JobDetected; void StartMonitoring(); void StopMonitoring(); }

// IPdfMerger.cs  
namespace PrintAssistant.Services.Abstractions;  
public interface IPdfMerger { Task\<(Stream MergedPdfStream, int TotalPages)\> MergePdfsAsync(IEnumerable\<Stream\> pdfStreams); }

// IPrintQueue.cs  
namespace PrintAssistant.Services.Abstractions;  
using PrintAssistant.Core;  
using System.Threading.Tasks.Dataflow;  
public interface IPrintQueue { Task EnqueueJobAsync(PrintJob job); Task\<PrintJob\> DequeueJobAsync(CancellationToken cancellationToken); IReceivableSourceBlock\<PrintJob\> AsReceivableSourceBlock(); }

// IPrintService.cs  
namespace PrintAssistant.Services.Abstractions;  
public interface IPrintService { Task PrintPdfAsync(Stream pdfStream, string printerName, int copies); }

// ITrayIconService.cs  
namespace PrintAssistant.Services.Abstractions;  
using PrintAssistant.Core;  
public interface ITrayIconService { void UpdateStatus(IEnumerable\<PrintJob\> recentJobs); void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon); }

#### **Services/Converters/ (所有转换器)**

C\#

// ExcelToPdfConverter.cs  
namespace PrintAssistant.Services.Converters;

using PrintAssistant.Services.Abstractions;  
using Syncfusion.Pdf;  
using Syncfusion.XlsIO;  
using Syncfusion.XlsIORenderer;  
using System.IO.Abstractions;

public class ExcelToPdfConverter(IFileSystem fileSystem) : IFileConverter  
{  
    private readonly IFileSystem \_fileSystem \= fileSystem;

    public async Task\<Stream\> ConvertToPdfAsync(string sourceFilePath)  
    {  
        using var excelEngine \= new ExcelEngine();  
        var application \= excelEngine.Excel;  
        await using var fileStream \= \_fileSystem.File.OpenRead(sourceFilePath);  
          
        var workbook \= application.Workbooks.Open(fileStream);  
        var renderer \= new XlsIORenderer();  
          
        // 配置转换选项，例如将所有列适应到一页  
        var settings \= new XlsIORendererSettings  
        {  
            LayoutOptions \= LayoutOptions.FitAllColumnsOnOnePage  
        };

        var pdfDocument \= renderer.ConvertToPDF(workbook, settings);  
          
        var pdfStream \= new MemoryStream();  
        pdfDocument.Save(pdfStream);  
        pdfStream.Position \= 0;  
          
        return pdfStream;  
    }  
}

// ImageToPdfConverter.cs  
namespace PrintAssistant.Services.Converters;

using PrintAssistant.Services.Abstractions;  
using Syncfusion.Drawing;  
using Syncfusion.Pdf;  
using Syncfusion.Pdf.Graphics;  
using System.IO.Abstractions;

public class ImageToPdfConverter(IFileSystem fileSystem) : IFileConverter  
{  
    private readonly IFileSystem \_fileSystem \= fileSystem;

    public async Task\<Stream\> ConvertToPdfAsync(string sourceFilePath)  
    {  
        using var document \= new PdfDocument();  
        var page \= document.Pages.Add();  
          
        await using var fileStream \= \_fileSystem.File.OpenRead(sourceFilePath);  
        var image \= new PdfBitmap(fileStream);

        // 在PDF页面上绘制图片  
        page.Graphics.DrawImage(image, new PointF(0, 0), new SizeF(page.GetClientSize().Width, page.GetClientSize().Height));  
          
        var pdfStream \= new MemoryStream();  
        document.Save(pdfStream);  
        pdfStream.Position \= 0;  
          
        return pdfStream;  
    }  
}

// WordToPdfConverter.cs  
namespace PrintAssistant.Services.Converters;

using PrintAssistant.Services.Abstractions;  
using Syncfusion.DocIO;  
using Syncfusion.DocIO.DLS;  
using Syncfusion.DocIORenderer;  
using Syncfusion.Pdf;  
using System.IO.Abstractions;

public class WordToPdfConverter(IFileSystem fileSystem) : IFileConverter  
{  
    private readonly IFileSystem \_fileSystem \= fileSystem;

    public async Task\<Stream\> ConvertToPdfAsync(string sourceFilePath)  
    {  
        await using var fileStream \= \_fileSystem.File.OpenRead(sourceFilePath);  
        using var wordDocument \= new WordDocument(fileStream, FormatType.Automatic);  
        using var renderer \= new DocIORenderer();  
          
        var pdfDocument \= renderer.ConvertToPDF(wordDocument);  
          
        var pdfStream \= new MemoryStream();  
        pdfDocument.Save(pdfStream);  
        pdfStream.Position \= 0;  
          
        return pdfStream;  
    }  
}

#### **Services/CoverPageGenerator.cs**

C\#

using System.Globalization;
using PrintAssistant.Core;  
using PrintAssistant.Services.Abstractions;  
using Syncfusion.Pdf;  
using Syncfusion.Pdf.Graphics;  

namespace PrintAssistant.Services;

public class CoverPageGenerator : ICoverPageGenerator  
{  
    public Task<Stream> GenerateCoverPageAsync(PrintJob job)
    {
        if (job == null)
        {
            throw new ArgumentNullException(nameof(job));
        }

        var document = new PdfDocument();
        var page = document.Pages.Add();
        var graphics = page.Graphics;

        var titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 20, PdfFontStyle.Bold);
        var bodyFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12);

        var culture = CultureInfo.CurrentCulture;

        float currentY = 40;
        graphics.DrawString("打印任务封面", titleFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(40, currentY));
        currentY += 40;

        graphics.DrawString($"任务编号: {job.JobId}", bodyFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(40, currentY));
        currentY += 25;

        graphics.DrawString($"创建时间: {job.CreationTime.ToString("f", culture)}", bodyFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(40, currentY));
        currentY += 25;

        graphics.DrawString($"文件数量: {job.SourceFilePaths.Count}", bodyFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(40, currentY));
        currentY += 25;

        graphics.DrawString("文件列表:", bodyFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(40, currentY));
        currentY += 20;

        foreach (var file in job.SourceFilePaths)  
        {  
            var name = Path.GetFileName(file);
            graphics.DrawString($"- {name}", bodyFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(60, currentY));
            currentY += 18;
        }

        var stream = new MemoryStream();
        document.Save(stream);  
        stream.Position = 0;
        document.Close(true);

        return Task.FromResult<Stream>(stream);
    }  
}

#### **Services/FileArchiver.cs**

C\#

using Microsoft.Extensions.Options;  
using PrintAssistant.Configuration;  
using PrintAssistant.Services.Abstractions;  
using System.IO.Abstractions;

namespace PrintAssistant.Services;

public class FileArchiver(IFileSystem fileSystem, IOptions<AppSettings> appSettings) : IFileArchiver
{
    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly AppSettings _settings = appSettings.Value;

    public async Task<string> ArchiveFilesAsync(IEnumerable<string> sourceFiles, DateTime jobCreationTime, Stream? mergedPdfStream = null, string? mergedFileName = null)
    {
        var monitorPath = GetMonitorPath();
        var archiveSubDirName = string.Format(_settings.Archiving.SubdirectoryFormat, jobCreationTime);
        var archivePath = _fileSystem.Path.Combine(monitorPath, archiveSubDirName);

        _fileSystem.Directory.CreateDirectory(archivePath);

        foreach (var sourceFile in sourceFiles)  
        {  
            if (_fileSystem.File.Exists(sourceFile))
            {
                var destFileName = _fileSystem.Path.Combine(archivePath, _fileSystem.Path.GetFileName(sourceFile));
                _fileSystem.File.Copy(sourceFile, destFileName, overwrite: true);
            }
        }

        if (mergedPdfStream != null)
        {
            mergedPdfStream.Position = 0;
            var targetName = string.IsNullOrWhiteSpace(mergedFileName)
                ? $"Merged_{jobCreationTime:yyyyMMdd_HHmmss}.pdf"
                : mergedFileName!;
            var mergedPath = _fileSystem.Path.Combine(archivePath, targetName);
            await using var fileStream = _fileSystem.File.Create(mergedPath);
            await mergedPdfStream.CopyToAsync(fileStream);
        }

        return archivePath;
    }

    public void MoveUnsupportedFile(string sourceFile)  
    {  
        var unsupportedPath = GetUnsupportedFilesPath();
        _fileSystem.Directory.CreateDirectory(unsupportedPath);

        if (_fileSystem.File.Exists(sourceFile))
        {  
            var destFileName = _fileSystem.Path.Combine(unsupportedPath, _fileSystem.Path.GetFileName(sourceFile));
            _fileSystem.File.Move(sourceFile, destFileName);
        }  
    }

    private string GetMonitorPath()  
    {  
        return !string.IsNullOrEmpty(_settings.Monitoring.Path)
            ? _settings.Monitoring.Path
            : _fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PrintJobs");
    }

    private string GetUnsupportedFilesPath()  
    {  
        return !string.IsNullOrEmpty(_settings.UnsupportedFiles.MoveToPath)
            ? _settings.UnsupportedFiles.MoveToPath
            : _fileSystem.Path.Combine(GetMonitorPath(), "Unsupported");
    }  
}

#### **Services/FileConverterFactory.cs**

C\#

using PrintAssistant.Services.Abstractions;  
using PrintAssistant.Services.Converters;

namespace PrintAssistant.Services;

public class FileConverterFactory(IServiceProvider serviceProvider) : IFileConverterFactory  
{  
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IFileConverter? GetConverter(string filePath)  
    {  
        var extension = Path.GetExtension(filePath).ToLowerInvariant();  
        return extension switch  
        {  
            ".doc" or ".docx" => _serviceProvider.GetService(typeof(WordToPdfConverter)) as IFileConverter,  
            ".xls" or ".xlsx" or ".xlsm" => _serviceProvider.GetService(typeof(ExcelToPdfConverter)) as IFileConverter,  
            ".jpg" or ".jpeg" or ".png" or ".bmp" => _serviceProvider.GetService(typeof(ImageToPdfConverter)) as IFileConverter,  
            ".pdf" => new PassthroughConverter(), // 对于PDF文件，我们不需要转换  
            _ => null,  
        };  
    }

    /// <summary>  
    /// 一个特殊的“转换器”，用于处理已经是PDF的文件。它只是简单地将源文件流复制出来。  
    /// </summary>  
    private class PassthroughConverter : IFileConverter  
    {  
        public async Task<Stream> ConvertToPdfAsync(string sourceFilePath)  
        {  
            var memoryStream = new MemoryStream();  
            await using (var fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))  
            {  
                await fileStream.CopyToAsync(memoryStream);  
            }  
            memoryStream.Position = 0;  
            return memoryStream;  
        }  
    }  
}

#### **Services/FileMonitorService.cs**

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using Microsoft.Extensions.Logging;  
using Microsoft.Extensions.Options;  
using PrintAssistant.Configuration;  
using PrintAssistant.Core;  
using PrintAssistant.Services.Abstractions;  
using System.IO.Abstractions;  

namespace PrintAssistant.Services
{
public class FileMonitorService : IFileMonitor, IDisposable  
{  
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<FileMonitorService> _logger;
        private readonly MonitorSettings _settings;

        private readonly HashSet<string> _pendingFiles = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _syncRoot = new();

        private FileSystemWatcher? _watcher;
        private System.Timers.Timer? _debounceTimer;
        private string _monitorPath = string.Empty;
        private bool _disposed;

        public event Action<PrintJob>? JobDetected;

        public FileMonitorService(
            IOptions<AppSettings> appSettings,
            IFileSystem fileSystem,
            ILogger<FileMonitorService> logger)
        {
            if (appSettings == null) throw new ArgumentNullException(nameof(appSettings));

            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = appSettings.Value.Monitoring ?? new MonitorSettings();
        }

        public void StartMonitoring()
        {
            ThrowIfDisposed();

            if (_watcher != null)
            {
                return;
            }

            _monitorPath = DetermineMonitorPath();
            _fileSystem.Directory.CreateDirectory(_monitorPath);

            _debounceTimer = new System.Timers.Timer(_settings.DebounceIntervalMilliseconds)
            {
                AutoReset = false,
            };
            _debounceTimer.Elapsed += (_, _) => FlushPendingFiles();

            _watcher = CreateWatcher(_monitorPath);
            _watcher.Created += OnWatcherEvent;
            _watcher.Changed += OnWatcherEvent;
            _watcher.Renamed += OnWatcherRenamed;
            _watcher.Error += OnWatcherError;
            _watcher.EnableRaisingEvents = true;

            _logger.LogInformation("File monitor started at {MonitorPath}", _monitorPath);
        }

        public void StopMonitoring()
        {
            if (_watcher == null)
            {
                return;
            }

            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= OnWatcherEvent;
            _watcher.Changed -= OnWatcherEvent;
            _watcher.Renamed -= OnWatcherRenamed;
            _watcher.Error -= OnWatcherError;
            _watcher.Dispose();
            _watcher = null;

            if (_debounceTimer != null)
            {
                _debounceTimer.Stop();
                _debounceTimer.Dispose();
                _debounceTimer = null;
            }

            lock (_syncRoot)
            {
                _pendingFiles.Clear();
            }

            _logger.LogInformation("File monitor stopped for {MonitorPath}", _monitorPath);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            StopMonitoring();
            _disposed = true;
        }

        internal void HandleFileEvent(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            try
            {
                string fullPath = _fileSystem.Path.GetFullPath(filePath);

                if (!_fileSystem.File.Exists(fullPath))
                {
                    return;
                }

                if (!IsWithinMonitoredDirectory(fullPath))
                {
                    return;
                }

                if (ExceedsSizeLimit(fullPath))
                {
                    _logger.LogWarning("Ignoring file '{FilePath}' because it exceeds the size limit of {Limit} MB.", fullPath, _settings.MaxFileSizeMegaBytes);
                    return;
                }

                lock (_syncRoot)
                {
                    _pendingFiles.Add(fullPath);
                }

                RestartTimer();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while handling file event for {FilePath}", filePath);
            }
        }

        internal void FlushPendingFiles()
        {
            List<string> files;

            lock (_syncRoot)
            {
                if (_pendingFiles.Count == 0)
                {
                    return;
                }

                files = _pendingFiles.ToList();
                _pendingFiles.Clear();
            }

            files = files.Where(_fileSystem.File.Exists).ToList();
            if (files.Count == 0)
            {
                return;
            }

            var job = new PrintJob(files);
            JobDetected?.Invoke(job);

            _logger.LogInformation("Detected new print job {JobId} with {FileCount} files.", job.JobId, files.Count);
        }

        private void OnWatcherEvent(object sender, FileSystemEventArgs e) => HandleFileEvent(e.FullPath);

        private void OnWatcherRenamed(object sender, RenamedEventArgs e) => HandleFileEvent(e.FullPath);

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            _logger.LogError(e.GetException(), "FileSystemWatcher encountered an error. Attempting to restart monitoring.");
            StopMonitoring();
            StartMonitoring();
        }

        private bool ExceedsSizeLimit(string path)
        {
            if (_settings.MaxFileSizeMegaBytes <= 0)
            {
                return false;
            }

            long sizeLimitBytes = _settings.MaxFileSizeMegaBytes * 1024L * 1024L;
            var info = _fileSystem.FileInfo.New(path);
            return info.Exists && info.Length > sizeLimitBytes;
        }

        private bool IsWithinMonitoredDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(_monitorPath))
            {
                return false;
            }

            var normalizedMonitor = _fileSystem.Path.GetFullPath(_monitorPath).TrimEnd(_fileSystem.Path.DirectorySeparatorChar);
            var normalizedPath = _fileSystem.Path.GetFullPath(path);

            return normalizedPath.StartsWith(normalizedMonitor + _fileSystem.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(normalizedPath, normalizedMonitor, StringComparison.OrdinalIgnoreCase);
        }

        private void RestartTimer()
        {
            if (_debounceTimer == null)
            {
                return;
            }

            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private string DetermineMonitorPath()
        {
            if (!string.IsNullOrWhiteSpace(_settings.Path))
            {
                return _settings.Path;
            }

            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            return _fileSystem.Path.Combine(desktop, "PrintJobs");
        }

        private FileSystemWatcher CreateWatcher(string path)
        {
            return new FileSystemWatcher(path)
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.*"
            };
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(FileMonitorService));
            }
        }
    }
}
```

#### **Services/PdfMerger.cs**

C\#

using PrintAssistant.Services.Abstractions;  
using Syncfusion.Pdf;  
using Syncfusion.Pdf.Parsing;

namespace PrintAssistant.Services;

public class PdfMerger : IPdfMerger  
{  
    public async Task<(Stream MergedPdfStream, int TotalPages)> MergePdfsAsync(IEnumerable<Stream> pdfStreams)
    {
        if (pdfStreams == null)
        {
            throw new ArgumentNullException(nameof(pdfStreams));
        }

        var streams = pdfStreams.Where(stream => stream != null).ToList();
        if (streams.Count == 0)
        {
            return (Stream.Null, 0);
        }

        using var mergedDocument = new PdfDocument();

        foreach (var stream in streams)
        {
            stream.Position = 0;
            using var loadedDocument = new PdfLoadedDocument(stream);
            mergedDocument.Append(loadedDocument);
        }

        var resultStream = new MemoryStream();
        mergedDocument.Save(resultStream);
        await resultStream.FlushAsync().ConfigureAwait(false);
        resultStream.Position = 0;

        var totalPages = mergedDocument.Pages.Count;
        mergedDocument.Close(true);

        return (resultStream, totalPages);
    }  
}

#### **Services/PrintProcessorService.cs**

```csharp
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;

namespace PrintAssistant.Services;

public class PrintProcessorService : BackgroundService
{
    private readonly ILogger<PrintProcessorService> _logger;
    private readonly IPrintQueue _printQueue;
    private readonly IFileMonitor _fileMonitor;
    private readonly ITrayIconService _trayIconService;
    private readonly IPrintService _printService;
    private readonly IFileConverterFactory _fileConverterFactory;
    private readonly IPdfMerger _pdfMerger;
    private readonly IFileArchiver _fileArchiver;
    private readonly ICoverPageGenerator _coverPageGenerator;
    private readonly AppSettings _appSettings;

    private readonly ConcurrentDictionary<Guid, PrintJob> _recentJobs = new();

    public PrintProcessorService(
        ILogger<PrintProcessorService> logger,
        IPrintQueue printQueue,
        IFileMonitor fileMonitor,
        ITrayIconService trayIconService,
        IPrintService printService,
        IFileConverterFactory fileConverterFactory,
        IPdfMerger pdfMerger,
        IFileArchiver fileArchiver,
        ICoverPageGenerator coverPageGenerator,
        IOptions<AppSettings> appSettings)
    {
        _logger = logger;
        _printQueue = printQueue;
        _fileMonitor = fileMonitor;
        _trayIconService = trayIconService;
        _printService = printService;
        _fileConverterFactory = fileConverterFactory;
        _pdfMerger = pdfMerger;
        _fileArchiver = fileArchiver;
        _coverPageGenerator = coverPageGenerator;
        _appSettings = appSettings.Value;

        _fileMonitor.JobDetected += OnJobDetected;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _fileMonitor.StartMonitoring();
        _trayIconService.ShowBalloonTip(2000, "PrintAssistant", "打印助手已启动，正在监控文件夹", ToolTipIcon.Info);
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _fileMonitor.StopMonitoring();
        return base.StopAsync(cancellationToken);
    }

    private async void OnJobDetected(PrintJob job)
    {
        _recentJobs[job.JobId] = job;
        _trayIconService.UpdateStatus(_recentJobs.Values.OrderByDescending(j => j.CreationTime).Take(5));

        try
        {
        await _printQueue.EnqueueJobAsync(job).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue job {JobId}", job.JobId);
            job.Status = JobStatus.Failed;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            PrintJob? job = null;
            try
            {
                job = await _printQueue.DequeueJobAsync(stoppingToken).ConfigureAwait(false);
                await ProcessJobAsync(job, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (job != null)
                {
                    job.Status = JobStatus.Failed;
                    job.ErrorMessage = ex.Message;
                }

                _logger.LogError(ex, "Error processing print job.");
            }
            finally
            {
                _trayIconService.UpdateStatus(_recentJobs.Values);
            }
        }
    }

    private async Task ProcessJobAsync(PrintJob job, CancellationToken cancellationToken)
    {
        job.Status = JobStatus.Converting;
        _trayIconService.UpdateStatus(_recentJobs.Values);

        var pdfStreams = new List<Stream>();
        var disposableStreams = new List<Stream>();
        try
        {
            foreach (var filePath in job.SourceFilePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var converter = _fileConverterFactory.GetConverter(filePath);
                if (converter == null)
                {
                    _logger.LogWarning("File {FilePath} is not supported and will be moved to unsupported directory.", filePath);
                    _fileArchiver.MoveUnsupportedFile(filePath);
                    continue;
                }

                try
                {
                    var pdfStream = await converter.ConvertToPdfAsync(filePath).ConfigureAwait(false);
                    pdfStreams.Add(pdfStream);
                    disposableStreams.Add(pdfStream);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to convert file {FilePath} to PDF.", filePath);
                    throw;
                }
            }

            if (pdfStreams.Count == 0)
            {
                throw new InvalidOperationException("No supported files were available for printing.");
            }

            if (_appSettings.Printing.GenerateCoverPage)
            {
                var coverPageStream = await _coverPageGenerator.GenerateCoverPageAsync(job).ConfigureAwait(false);
                pdfStreams.Insert(0, coverPageStream);
                disposableStreams.Add(coverPageStream);
            }

            var (mergedStream, totalPages) = await _pdfMerger.MergePdfsAsync(pdfStreams).ConfigureAwait(false);
            disposableStreams.Add(mergedStream);

            job.PageCount = totalPages;
            job.Status = JobStatus.Printing;
            _trayIconService.UpdateStatus(_recentJobs.Values);

            mergedStream.Position = 0;
            await _printService.PrintPdfAsync(mergedStream, job.SelectedPrinter ?? string.Empty, job.Copies).ConfigureAwait(false);

            job.Status = JobStatus.Completed;
            _logger.LogInformation("Job {JobId} printed successfully with {PageCount} pages.", job.JobId, job.PageCount);

            mergedStream.Position = 0;
            var archivePath = await _fileArchiver.ArchiveFilesAsync(job.SourceFilePaths, job.CreationTime, mergedStream, $"{job.JobId}.pdf").ConfigureAwait(false);
            job.Status = JobStatus.Archived;
            _logger.LogInformation("Job {JobId} archived at {ArchivePath}.", job.JobId, archivePath);

            _trayIconService.ShowBalloonTip(2000, "打印完成", $"任务 {job.JobId} 已完成并归档。", ToolTipIcon.Info);
        }
        finally
        {
            foreach (var stream in disposableStreams)
            {
                stream.Dispose();
            }

            _trayIconService.UpdateStatus(_recentJobs.Values);
        }
    }
}
```

#### **Services/PrintQueueService.cs**

```csharp
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using PrintAssistant.Core;  
using PrintAssistant.Services.Abstractions;  

namespace PrintAssistant.Services;

public class PrintQueueService : IPrintQueue  
{  
    private readonly BufferBlock<PrintJob> _queue;
    private readonly ILogger<PrintQueueService> _logger;

    public PrintQueueService(ILogger<PrintQueueService> logger)
    {
        _logger = logger;
        _queue = new BufferBlock<PrintJob>(new DataflowBlockOptions
        {
            BoundedCapacity = DataflowBlockOptions.Unbounded
        });
    }

    public async Task EnqueueJobAsync(PrintJob job)
    {
        if (job == null)
        {
            throw new ArgumentNullException(nameof(job));
        }

        await _queue.SendAsync(job).ConfigureAwait(false);
        _logger.LogInformation("Job {JobId} enqueued.", job.JobId);
    }

    public Task<PrintJob> DequeueJobAsync(CancellationToken cancellationToken) => _queue.ReceiveAsync(cancellationToken);

    public IReceivableSourceBlock<PrintJob> AsReceivableSourceBlock() => _queue;
}
```

#### **Services/PrintService.cs**

```csharp
using Microsoft.Extensions.Logging;
using PrintAssistant.Services.Abstractions;  
using Syncfusion.PdfToImageConverter;  
using System.Drawing.Printing;

namespace PrintAssistant.Services;

public class PrintService : IPrintService  
{  
    private readonly ILogger<PrintService> _logger;

    public PrintService(ILogger<PrintService> logger)
    {
        _logger = logger;
    }

    public Task PrintPdfAsync(Stream pdfStream, string printerName, int copies)
    {
        _logger.LogInformation(
            "PrintService: 打印PDF文档。打印机: {PrinterName}, 份数: {Copies}, 数据流大小: {StreamLength} bytes.",
            printerName,
            copies,
            pdfStream.Length);

        using var printDocument = new PrintDocument();
        printDocument.PrinterSettings.PrinterName = printerName;
        printDocument.PrintController = new StandardPrintController();
        printDocument.Print();

        return Task.CompletedTask;
    }
}
```

#### **Services/TrayIconService.cs**

```csharp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;  
using Microsoft.Extensions.Logging;  
using PrintAssistant.Core;  
using PrintAssistant.Services.Abstractions;  

namespace PrintAssistant.Services;

public class TrayIconService : ITrayIconService, IDisposable  
{  
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TrayIconService> _logger;
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private bool _disposed;

    public TrayIconService(IServiceProvider serviceProvider, ILogger<TrayIconService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _contextMenu = BuildContextMenu();

        _notifyIcon = new NotifyIcon
        {
            Text = "PrintAssistant",
            Icon = SystemIcons.Application,
            Visible = true,
            ContextMenuStrip = _contextMenu,
        };
    }

    public void UpdateStatus(IEnumerable<PrintJob> recentJobs)
    {
        var jobList = recentJobs.ToList();

        string tooltip = jobList.Count == 0
            ? "打印助手：暂无任务"
            : string.Join(Environment.NewLine, jobList.Take(5).Select(j => $"[{j.Status}] {string.Join(", ", j.SourceFilePaths.Select(Path.GetFileName))}"));

        _notifyIcon.Text = tooltip.Length <= 63 ? tooltip : tooltip[..63];

        _logger.LogDebug("Tray icon status updated with {Count} jobs", jobList.Count);
    }

    public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
        => _notifyIcon.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();

        var settingsItem = new ToolStripMenuItem("设置(&S)", null, OnSettingsClicked);
        var exitItem = new ToolStripMenuItem("退出(&E)", null, OnExitClicked);

        menu.Items.Add(settingsItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        return menu;
    }

    private void OnSettingsClicked(object? sender, EventArgs e)
    {
        using var scope = _serviceProvider.CreateScope();
        var form = scope.ServiceProvider.GetRequiredService<PrintAssistant.UI.SettingsForm>();
        form.ShowDialog();
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        _notifyIcon.Visible = false;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenu.Dispose();
        _disposed = true;
    }
}
```

#### **Services/MockPrintService.cs**

```csharp
using Microsoft.Extensions.Logging;
using PrintAssistant.Services.Abstractions;

namespace PrintAssistant.Services;

public class MockPrintService : IPrintService
{
    private readonly ILogger<MockPrintService> _logger;

    public MockPrintService(ILogger<MockPrintService> logger)
    {
        _logger = logger;
    }

    public Task PrintPdfAsync(Stream pdfStream, string printerName, int copies)
    {
        _logger.LogInformation(
            "MockPrintService: 模拟打印任务。打印机: {PrinterName}, 份数: {Copies}, 数据流大小: {StreamLength} bytes.",
            printerName,
            copies,
            pdfStream.Length);

        return Task.Delay(1000);
    }
}
```

#### **UI/PrinterSelectionForm.cs**

```csharp
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace PrintAssistant.UI;

public partial class PrinterSelectionForm : Form
{
    public string? SelectedPrinter { get; private set; }
    public int PrintCopies { get; private set; }

    private readonly List<string> _excludedPrinters;

    public PrinterSelectionForm(List<string> excludedPrinters)
    {
        InitializeComponent();
        _excludedPrinters = excludedPrinters;
        TopMost = true;
    }

    private void PrinterSelectionForm_Load(object sender, EventArgs e) => LoadPrinters();

    private void LoadPrinters()
    {
        string defaultPrinter = new PrinterSettings().PrinterName;

        foreach (string printer in PrinterSettings.InstalledPrinters.Cast<string>())
        {
            if (!_excludedPrinters.Contains(printer, StringComparer.OrdinalIgnoreCase))
            {
                cmbPrinters.Items.Add(printer);
            }
        }

        if (cmbPrinters.Items.Contains(defaultPrinter))
        {
            cmbPrinters.SelectedItem = defaultPrinter;
        }
        else if (cmbPrinters.Items.Count > 0)
        {
            cmbPrinters.SelectedIndex = 0;
        }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        if (cmbPrinters.SelectedItem == null)
        {
            MessageBox.Show("请选择一个打印机。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SelectedPrinter = cmbPrinters.SelectedItem.ToString();
        PrintCopies = (int)numCopies.Value;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
```

#### **UI/PrinterSelectionForm.Designer.cs**

```csharp
using System.Drawing;
using System.Windows.Forms;

namespace PrintAssistant.UI
{
    partial class PrinterSelectionForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblPrinter = new Label();
            cmbPrinters = new ComboBox();
            lblCopies = new Label();
            numCopies = new NumericUpDown();
            btnOK = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)numCopies).BeginInit();
            SuspendLayout();
            // ... Designer generated code ...
            Load += PrinterSelectionForm_Load;
            ((System.ComponentModel.ISupportInitialize)numCopies).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Label lblPrinter;
        private ComboBox cmbPrinters;
        private Label lblCopies;
        private NumericUpDown numCopies;
        private Button btnOK;
        private Button btnCancel;
    }
}
```

#### **UI/SettingsForm.cs**

```csharp
using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using System;
using System.Drawing.Printing;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace PrintAssistant.UI;

public partial class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly IFileSystem _fileSystem;
    private readonly string _appSettingsPath;

    public SettingsForm(IOptions<AppSettings> appSettings, IFileSystem fileSystem)
    {
        InitializeComponent();
        _settings = appSettings.Value;
        _fileSystem = fileSystem;
        _appSettingsPath = _fileSystem.Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    }

    private void SettingsForm_Load(object sender, EventArgs e) => LoadSettings();

    private void LoadSettings()
    {
        txtMonitorPath.Text = _settings.Monitoring.Path;
        numDebounceInterval.Value = _settings.Monitoring.DebounceIntervalMilliseconds;
        numMaxFileSize.Value = _settings.Monitoring.MaxFileSizeMegaBytes;
        chkGenerateCoverPage.Checked = _settings.Printing.GenerateCoverPage;
        LoadPrintersForExclusion();
        txtLogPath.Text = _settings.Logging.Path;
    }

    private void LoadPrintersForExclusion()
    {
        clbExcludedPrinters.Items.Clear();
        foreach (string printer in PrinterSettings.InstalledPrinters.Cast<string>())
        {
            bool isExcluded = _settings.Printing.ExcludedPrinters.Contains(printer, StringComparer.OrdinalIgnoreCase);
            clbExcludedPrinters.Items.Add(printer, isExcluded);
        }
    }

    private void btnBrowseMonitorPath_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog { Description = "请选择要监控的文件夹" };
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtMonitorPath.Text = dialog.SelectedPath;
        }
    }

    private void btnBrowseLogPath_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog { Description = "请选择日志文件存储位置" };
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtLogPath.Text = dialog.SelectedPath;
        }
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        SaveChanges();
        MessageBox.Show("设置已保存。部分设置可能需要重启应用才能生效。", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }

    private void SaveChanges()
    {
        _settings.Monitoring.Path = txtMonitorPath.Text;
        _settings.Monitoring.DebounceIntervalMilliseconds = (int)numDebounceInterval.Value;
        _settings.Monitoring.MaxFileSizeMegaBytes = (long)numMaxFileSize.Value;
        _settings.Printing.GenerateCoverPage = chkGenerateCoverPage.Checked;
        _settings.Printing.ExcludedPrinters.Clear();
        foreach (var item in clbExcludedPrinters.CheckedItems)
        {
            _settings.Printing.ExcludedPrinters.Add(item.ToString()!);
        }
        _settings.Logging.Path = txtLogPath.Text;

        var jsonString = _fileSystem.File.ReadAllText(_appSettingsPath);
        var jsonDocument = JsonDocument.Parse(jsonString, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        var root = jsonDocument.RootElement.Clone();
        var appSettingsJson = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
        var newAppSettingsNode = JsonDocument.Parse(appSettingsJson).RootElement;

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            foreach (var property in root.EnumerateObject())
            {
                if (property.NameEquals("AppSettings"))
                {
                    writer.WritePropertyName("AppSettings");
                    newAppSettingsNode.WriteTo(writer);
                }
                else
                {
                    property.WriteTo(writer);
                }
            }
            writer.WriteEndObject();
        }

        var newJsonString = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        _fileSystem.File.WriteAllText(_appSettingsPath, newJsonString);
    }

    private void btnCancel_Click(object sender, EventArgs e) => Close();
}
```

#### **UI/SettingsForm.Designer.cs**

```csharp
using System.Drawing;
using System.Windows.Forms;

namespace PrintAssistant.UI
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

* **PrinterSelectionForm.cs**:  
  * 在构造函数中接收打印机列表和排除列表。  
  * 在 Load 事件中，填充打印机 ComboBox。  
  * 提供公共属性 SelectedPrinter 和 PrintCopies 以返回用户选择。  
  * 处理“确定”和“取消”按钮的点击事件，设置 DialogResult。  
* **SettingsForm.cs**:  
  * 在构造函数中注入 IOptions\<AppSettings\> 和 IFileSystem。  
  * 在 Load 事件中，用配置值填充UI控件（文本框、复选框列表等）。  
  * 为“浏览”按钮添加事件处理，打开 FolderBrowserDialog。  
  * 在“保存”按钮的点击事件中，读取UI控件的值，更新 AppSettings 对象，然后将其序列化写回 appsettings.json 文件。

---

### **2\. 单元测试 (tests/PrintAssistant.Tests/)**

#### **PrintAssistant.Tests.csproj**

XML

\<Project Sdk\="Microsoft.NET.Sdk"\>

  \<PropertyGroup\>  
    \<TargetFramework\>net8.0-windows\</TargetFramework\>  
    \<ImplicitUsings\>enable\</ImplicitUsings\>  
    \<Nullable\>enable\</Nullable\>  
    \<IsPackable\>false\</IsPackable\>  
    \<UseWindowsForms\>true\</UseWindowsForms\>  
  \</PropertyGroup\>

  \<ItemGroup\>  
    \<PackageReference Include\="Microsoft.NET.Test.Sdk" Version\="17.8.0" /\>  
    \<PackageReference Include\="Moq" Version\="4.20.70" /\>  
    \<PackageReference Include\="System.IO.Abstractions.TestingHelpers" Version\="20.0.15" /\>  
    \<PackageReference Include\="xunit" Version\="2.5.3" /\>  
    \<PackageReference Include\="xunit.runner.visualstudio" Version\="2.5.3" /\>  
  \</ItemGroup\>

  \<ItemGroup\>  
    \<ProjectReference Include\="..\\..\\src\\PrintAssistant\\PrintAssistant.csproj" /\>  
  \</ItemGroup\>

\</Project\>

#### **Services/FileArchiverTests.cs**

C\#

using Microsoft.Extensions.Options;  
using PrintAssistant.Configuration;  
using PrintAssistant.Services;  
using System.IO.Abstractions.TestingHelpers;  
using Xunit;

namespace PrintAssistant.Tests.Services;

public class FileArchiverTests  
{  
    \[Fact\]  
    public async Task ArchiveFilesAsync\_ShouldMoveFilesToTimestampedDirectory()  
    {  
        // Arrange  
        var mockFileSystem \= new MockFileSystem();  
        var monitorPath \= "C:\\\\PrintJobs";  
        var sourceFilePath \= "C:\\\\PrintJobs\\\\test.txt";  
        mockFileSystem.AddFile(sourceFilePath, new MockFileData("test content"));

        var appSettings \= new AppSettings  
        {  
            Monitoring \= new MonitorSettings { Path \= monitorPath },  
            Archiving \= new ArchiveSettings { SubdirectoryFormat \= "Processed\_{0:yyyyMMdd}" }  
        };  
        var mockOptions \= Options.Create(appSettings);

        var archiver \= new FileArchiver(mockFileSystem, mockOptions);  
        var jobTime \= new DateTime(2023, 10, 27);

        // Act  
        await archiver.ArchiveFilesAsync(new { sourceFilePath }, jobTime);

        // Assert  
        var expectedArchivePath \= "C:\\\\PrintJobs\\\\Processed\_20231027\\\\test.txt";  
        Assert.False(mockFileSystem.FileExists(sourceFilePath));  
        Assert.True(mockFileSystem.FileExists(expectedArchivePath));  
    }  
}

#### **Services/FileMonitorServiceTests.cs**

*(这是一个更复杂的测试示例，用于演示如何测试防抖逻辑。)*

C\#

using Microsoft.Extensions.Logging.Abstractions;  
using Microsoft.Extensions.Options;  
using Moq;  
using PrintAssistant.Configuration;  
using PrintAssistant.Core;  
using PrintAssistant.Services;  
using PrintAssistant.Services.Abstractions;  
using System.IO.Abstractions.TestingHelpers;  
using Xunit;

namespace PrintAssistant.Tests.Services;

public class FileMonitorServiceTests  
{  
    \[Fact\]  
    public async Task FileMonitor\_ShouldDebounceMultipleEvents\_IntoSingleJob()  
    {  
        // Arrange  
        var mockFileSystem \= new MockFileSystem();  
        var monitorPath \= "C:\\\\PrintJobs";  
        mockFileSystem.AddDirectory(monitorPath);

        var appSettings \= new AppSettings { Monitoring \= new MonitorSettings { Path \= monitorPath, DebounceIntervalMilliseconds \= 100 } };  
        var mockOptions \= Options.Create(appSettings);  
        var logger \= NullLogger\<FileMonitorService\>.Instance;

        PrintJob? detectedJob \= null;  
          
        var monitor \= new FileMonitorService(mockFileSystem, mockOptions, logger);  
        monitor.JobDetected \+= (job) \=\> detectedJob \= job;  
          
        monitor.StartMonitoring();

        // Act  
        // 模拟快速连续创建多个文件  
        mockFileSystem.AddFile(Path.Combine(monitorPath, "file1.txt"), new MockFileData(""));  
        await Task.Delay(10);  
        mockFileSystem.AddFile(Path.Combine(monitorPath, "file2.txt"), new MockFileData(""));  
        await Task.Delay(10);  
        mockFileSystem.AddFile(Path.Combine(monitorPath, "file3.txt"), new MockFileData(""));

        // 等待防抖计时器触发  
        await Task.Delay(200);

        // Assert  
        Assert.NotNull(detectedJob);  
        Assert.Equal(3, detectedJob.SourceFilePaths.Count);  
        Assert.Contains(detectedJob.SourceFilePaths, p \=\> p.EndsWith("file1.txt"));  
        Assert.Contains(detectedJob.SourceFilePaths, p \=\> p.EndsWith("file2.txt"));  
        Assert.Contains(detectedJob.SourceFilePaths, p \=\> p.EndsWith("file3.txt"));  
    }  
}  
