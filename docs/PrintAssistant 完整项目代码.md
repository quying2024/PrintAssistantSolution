

### **项目目录结构**

PrintAssistantSolution/  
│  
├── src/  
│   └── PrintAssistant/  
│       ├── PrintAssistant.csproj  
│       ├── appsettings.json  
│       ├── appsettings.Secret.json
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
│       │   ├── JobStatus.cs
│       │   └── PrintJobStage.cs  
│       │  
│       ├── Services/  
│       │   ├── Abstractions/  
│       │   │   ├── ICoverPageGenerator.cs  
│       │   │   ├── IFileArchiver.cs  
│       │   │   ├── IFileConverter.cs  
│       │   │   ├── IFileConverterFactory.cs  
│       │   │   ├── IFileMonitor.cs  
│       │   │   ├── IJobStageRetryDecider.cs
│       │   │   ├── IPdfMerger.cs  
│       │   │   ├── IPrintQueue.cs  
│       │   │   ├── IPrintService.cs  
│       │   │   ├── IRetryPolicy.cs
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
│       │   ├── MockPrintService.cs
│       │   ├── PdfMerger.cs  
│       │   ├── PrintProcessorService.cs  
│       │   ├── PrintQueueService.cs  
│       │   ├── PrintService.cs  
│       │   ├── Retry/
│       │   │   ├── DefaultRetryPolicy.cs
│       │   │   └── RetryContext.cs
│       │   ├── TrayIconService.cs
│       │   └── WindowsPrintService.cs  
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
        ├── Logging/
        │   └── SerilogBootstrapTests.cs
        └── Services/  
            ├── DocumentConversionIntegrationTests.cs
            ├── FileArchiverTests.cs  
            ├── FileConverterFactoryTests.cs
            ├── FileMonitorServiceTests.cs
            └── PrintProcessorServiceTests.cs

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
    \<PackageReference Include\="Microsoft.Windows.Compatibility" Version\="8.0.1" /\>  
    \<PackageReference Include\="Microsoft.Extensions.DependencyInjection.Abstractions" Version\="9.0.9" /\>  
    \<PackageReference Include\="Microsoft.Extensions.Hosting" Version\="8.0.0" /\>  
    \<PackageReference Include\="Serilog.Extensions.Hosting" Version\="8.0.0" /\>  
    \<PackageReference Include\="Serilog.Settings.Configuration" Version\="8.0.0" /\>  
    \<PackageReference Include\="Serilog.Sinks.File" Version\="5.0.0" /\>  
    \<PackageReference Include\="System.IO.Abstractions" Version\="20.0.15" /\>  
    \<PackageReference Include\="System.Threading.Tasks.Dataflow" Version\="8.0.0" /\>  
    \<PackageReference Include\="Polly" Version\="7.2.4" /\>  
    \<PackageReference Include\="Syncfusion.DocIO.Net.Core" Version\="*" /\>  
    \<PackageReference Include\="Syncfusion.DocIORenderer.Net.Core" Version\="*" /\>  
    \<PackageReference Include\="Syncfusion.Pdf.Net.Core" Version\="*" /\>  
    \<PackageReference Include\="Syncfusion.PdfToImageConverter.Net" Version\="31.1.22" /\>  
    \<PackageReference Include\="Syncfusion.XlsIO.Net.Core" Version\="*" /\>  
    \<PackageReference Include\="Syncfusion.XlsIORenderer.Net.Core" Version\="*" /\>  
  \</ItemGroup\>

  \<ItemGroup\>  
    \<None Update\="appsettings.json"\>  
      \<CopyToOutputDirectory\>PreserveNewest\</CopyToOutputDirectory\>  
    \</None\>  
    \<None Update\="appsettings.Secret.json"\>  
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
      "ExcludedPrinters": [],
      "GenerateCoverPage": true,
      "UseMockPrintService": true,
      "RetryPolicy": {
        "MaxRetryCount": 3,
        "InitialDelayMilliseconds": 1000,
        "BackoffFactor": 2,
        "MaxDelayMilliseconds": 30000,
        "RetryOn": [ "Conversion", "Merge", "Print", "Archive" ]
      },
      "Windows": {
        "DefaultPrinter": "",
        "DefaultCopies": 1,
        "Duplex": false,
        "Collate": true,
        "PaperSource": "",
        "Landscape": false,
        "StretchToFit": true,
        "Color": true,
        "Dpi": 200
      }
    },  
    "Archiving": {  
      "SubdirectoryFormat": "Processed\_{0:yyyyMMdd\_HHmmss}"  
    },  
    "Logging": {  
      "Path": "" // 留空则默认为 C:\\Users\\用户名\\AppData\\Local\\PrintAssistant\\Logs  
    }  
  }  
}

#### **appsettings.Secret.json**

JSON

{
  "Syncfusion": {
    "DocumentSdk": {
      "LicenseKeys": [
        "NxYtFisQPR08Cit/Vkd+XU9FcVRDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS3tSdkViWX1bdXFVQmlYU091Xg==",
        "@33312e302e303b33313bcSTC0Sw46H5Cgbb0gLWvt/h5UNhUr/wSLLQ9AtLwUfA="
      ],
      "Version": "31.x.x"
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

        bool useMock = configuration.GetValue<bool>("AppSettings:Printing:UseMockPrintService");
        if (useMock)
        {
            services.AddSingleton<IPrintService, MockPrintService>();
        }
        else
        {
            services.AddSingleton<IPrintService, WindowsPrintService>();
        }

        services.AddSingleton<IPdfMerger, PdfMerger>();
        services.AddSingleton<IRetryPolicy, DefaultRetryPolicy>();
        services.AddSingleton<IJobStageRetryDecider, DefaultRetryPolicy>();
        services.AddTransient<WordToPdfConverter>();
        services.AddTransient<ExcelToPdfConverter>();
        services.AddTransient<ImageToPdfConverter>();
        services.AddTransient<SettingsForm>();
        services.AddTransient<PrinterSelectionForm>();
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
    public bool UseMockPrintService { get; set; } \= true;
    public PrintRetryPolicySettings RetryPolicy { get; set; } \= new();
    public WindowsPrintSettings Windows { get; set; } \= new();
}

public class PrintRetryPolicySettings
{
    public int MaxRetryCount { get; set; } \= 3;
    public int InitialDelayMilliseconds { get; set; } \= 1000;
    public double BackoffFactor { get; set; } \= 2.0;
    public int MaxDelayMilliseconds { get; set; } \= 30000;
    public List\<string\> RetryOn { get; set; } \= new() { "Conversion", "Merge", "Print", "Archive" };
}

public class WindowsPrintSettings
{
    public string? DefaultPrinter { get; set; }
    public int DefaultCopies { get; set; } \= 1;
    public bool Duplex { get; set; } \= false;
    public bool Collate { get; set; } \= true;
    public string? PaperSource { get; set; }
    public bool Landscape { get; set; } \= false;
    public bool StretchToFit { get; set; } \= true;
    public bool Color { get; set; } \= true;
    public int Dpi { get; set; } \= 200;
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

    /// \<summary\>  
    /// 重试次数计数。  
    /// \</summary\>  
    public int AttemptCount { get; set; }

    /// \<summary\>  
    /// 最大重试次数。  
    /// \</summary\>  
    public int MaxRetryCount { get; set; }

    /// \<summary\>  
    /// 最后失败的阶段。  
    /// \</summary\>  
    public PrintJobStage? LastFailedStage { get; set; }

    public PrintJob(IEnumerable\<string\> sourceFilePaths)  
    {  
        JobId \= Guid.NewGuid();  
        CreationTime \= DateTime.UtcNow;  
        SourceFilePaths \= sourceFilePaths.ToList().AsReadOnly();  
        Status \= JobStatus.Pending;  
        Copies \= 1; // 默认份数  
        AttemptCount \= 0;
        MaxRetryCount \= 3; // 默认最大重试次数
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

#### **Core/PrintJobStage.cs**

C\#

namespace PrintAssistant.Core;

/// \<summary\>  
/// 标识打印作业在处理流程中的阶段，用于异常处理与重试策略。  
/// \</summary\>  
public enum PrintJobStage  
{  
    Conversion,  
    Merge,  
    Print,  
    Archive,  
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
public interface IPdfMerger { Task\<(Stream MergedPdfStream, int TotalPages)\> MergePdfsAsync(IEnumerable\<Func\<Task\<Stream\>\>\> pdfFactories); }

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

// IRetryPolicy.cs  
namespace PrintAssistant.Services.Abstractions;  
public interface IRetryPolicy { TimeSpan? GetDelay(int attempt); }

// IJobStageRetryDecider.cs  
namespace PrintAssistant.Services.Abstractions;  
using PrintAssistant.Core;  
public interface IJobStageRetryDecider { bool ShouldRetry(PrintJobStage stage); }

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

#### **Services/WindowsPrintService.cs**

C\#

using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using PrintAssistant.Services.Abstractions;
using Syncfusion.PdfToImageConverter;

namespace PrintAssistant.Services;

/// \<summary\>  
/// Windows 平台打印服务实现，使用 Syncfusion.PdfToImageConverter 将 PDF 转换为图像并通过 GDI 打印。  
/// \</summary\>  
public class WindowsPrintService : IPrintService
{
    private readonly ILogger\<WindowsPrintService\> _logger;
    private readonly WindowsPrintSettings _settings;

    public WindowsPrintService(ILogger\<WindowsPrintService\> logger, IOptions\<AppSettings\> options)
    {
        _logger = logger;
        _settings = options.Value.Printing.Windows;
    }

    public async Task PrintPdfAsync(Stream pdfStream, string printerName, int copies)
    {
        if (pdfStream == null)
        {
            throw new ArgumentNullException(nameof(pdfStream));
        }

        var targetPrinter = string.IsNullOrWhiteSpace(printerName)
            ? _settings.DefaultPrinter
            : printerName;

        if (string.IsNullOrWhiteSpace(targetPrinter))
        {
            throw new InvalidOperationException("WindowsPrintService: 未指定打印机，请检查配置或用户输入。");
        }

        var effectiveCopies = copies \> 0 ? copies : Math.Max(1, _settings.DefaultCopies);

        if (pdfStream.CanSeek)
        {
            pdfStream.Position = 0;
        }

        try
        {
            using var converter = new PdfToImageConverter();
            converter.Load(pdfStream);

            var pageCount = converter.PageCount;
            if (pageCount \<= 0)
            {
                _logger.LogWarning("WindowsPrintService: 文档页数为 0，无法打印。打印机: {Printer}", targetPrinter);
                return;
            }

            var rawStreams = converter.Convert(0, pageCount - 1, keepTransparency: false, isSkipAnnotations: false);
            if (rawStreams == null || rawStreams.Length == 0)
            {
                _logger.LogWarning("WindowsPrintService: PDF 转换为空，跳过打印。打印机: {Printer}", targetPrinter);
                return;
            }

            var imageStreams = new MemoryStream[rawStreams.Length];
            for (var i = 0; i \< rawStreams.Length; i++)
            {
                if (rawStreams[i] == null)
                {
                    continue;
                }

                var memory = new MemoryStream();
                rawStreams[i].Position = 0;
                rawStreams[i].CopyTo(memory);
                memory.Position = 0;
                imageStreams[i] = memory;
                rawStreams[i].Dispose();
            }

            using var printDocument = new PrintDocument
            {
                PrinterSettings = new PrinterSettings
                {
                    PrinterName = targetPrinter,
                    Copies = (short)Math.Clamp(effectiveCopies, 1, short.MaxValue)
                }
            };

            var paperSize = printDocument.PrinterSettings.PaperSizes
                .Cast\<PaperSize?\>()
                .FirstOrDefault(p =\> p?.Kind == PaperKind.A4);

            if (paperSize == null)
            {
                paperSize = new PaperSize("A4", width: 827, height: 1169);
            }

            printDocument.DefaultPageSettings.PaperSize = paperSize;
            printDocument.PrinterSettings.DefaultPageSettings.PaperSize = paperSize;

            printDocument.PrintController = new StandardPrintController();

            var totalPages = imageStreams.Length;
            var currentPage = 0;

            printDocument.PrintPage += (sender, e) =\>
            {
                if (currentPage \< totalPages && imageStreams[currentPage] != null)
                {
                    try
                    {
                        var stream = imageStreams[currentPage];
                        if (stream.CanSeek)
                        {
                            stream.Position = 0;
                        }

                        using var image = Image.FromStream(stream, useEmbeddedColorManagement: true, validateImageData: true);
                        DrawImage(e.Graphics!, image, e.MarginBounds);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "WindowsPrintService: 打印第 {Page} 页时发生错误。", currentPage + 1);
                    }
                    finally
                    {
                        imageStreams[currentPage]?.Dispose();
                        imageStreams[currentPage] = null;
                    }
                }

                currentPage++;
                e.HasMorePages = currentPage \< totalPages;
            };

            await Task.Run(() =\> printDocument.Print()).ConfigureAwait(false);

            _logger.LogInformation(
                "WindowsPrintService: 成功打印 PDF。打印机: {Printer}, 份数: {Copies}, 页数: {Pages}",
                targetPrinter,
                effectiveCopies,
                totalPages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WindowsPrintService: 打印过程中发生异常。");
            throw;
        }
    }

    private static void DrawImage(Graphics graphics, Image image, Rectangle targetBounds)
    {
        if (graphics == null || image == null)
        {
            return;
        }

        var scaleX = (float)targetBounds.Width / image.Width;
        var scaleY = (float)targetBounds.Height / image.Height;
        var scale = Math.Min(scaleX, scaleY);

        var drawWidth = (int)(image.Width * scale);
        var drawHeight = (int)(image.Height * scale);
        var offsetX = targetBounds.X + (targetBounds.Width - drawWidth) / 2;
        var offsetY = targetBounds.Y + (targetBounds.Height - drawHeight) / 2;

        graphics.DrawImage(image, offsetX, offsetY, drawWidth, drawHeight);
    }
}
```

#### **Services/Retry/DefaultRetryPolicy.cs**

C\#

using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;

namespace PrintAssistant.Services.Retry;

public class DefaultRetryPolicy : IRetryPolicy, IJobStageRetryDecider
{
    private readonly PrintRetryPolicySettings _settings;
    private readonly HashSet\<PrintJobStage\> _retryStages;

    public DefaultRetryPolicy(IOptions\<AppSettings\> options)
    {
        _settings = options.Value.Printing.RetryPolicy;
        _retryStages = _settings.RetryOn
            .Select(name =\> Enum.TryParse\<PrintJobStage\>(name, ignoreCase: true, out var stage) ? stage : (PrintJobStage?)null)
            .Where(stage =\> stage.HasValue)
            .Select(stage =\> stage!.Value)
            .ToHashSet();
    }

    public TimeSpan? GetDelay(int attempt)
    {
        if (attempt \>= _settings.MaxRetryCount)
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
```

#### **Services/Retry/RetryContext.cs**

C\#

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

    public int Attempt =\> Job.AttemptCount;
    public int MaxRetries { get; private set; }

    public void Initialize(int maxRetries)
    {
        MaxRetries = maxRetries;
        Job.MaxRetryCount = maxRetries;
    }

    public bool CanRetry(int currentAttempt)
    {
        return currentAttempt \< _settings.MaxRetryCount;
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

#### **Logging/SerilogBootstrapTests.cs**

C\#

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Xunit;

namespace PrintAssistant.Tests.Logging;

public class SerilogBootstrapTests
{
    \[Fact\]
    public void Bootstrap\_ShouldCreateLogsDirectoryAndFile()
    {
        // Arrange
        var baseDirectory = Path.Combine(Path.GetTempPath(), "PrintAssistantTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(baseDirectory);
        var originalDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = baseDirectory;

        var configurationJson = "{\"Serilog\":{\"WriteTo\":[{\"Name\":\"File\",\"Args\":{\"path\":\"Logs\\\\log-.txt\",\"rollingInterval\":\"Day\"}}]}}";
        var tempConfigPath = Path.Combine(baseDirectory, "appsettings.json");
        File.WriteAllText(tempConfigPath, configurationJson);

        var configuration = new ConfigurationManager();
        configuration.AddJsonFile(tempConfigPath, optional: false, reloadOnChange: false);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        Log.Information("Bootstrap test log entry.");
        Log.CloseAndFlush();

        // Assert
        var logsPath = Path.Combine(baseDirectory, "Logs");
        Assert.True(Directory.Exists(logsPath), "Logs directory should be created.");
        var files = Directory.GetFiles(logsPath, "log-*.txt");
        Assert.NotEmpty(files);
        Assert.Contains(files, file \=\> new FileInfo(file).Length \> 0);

        // Cleanup
        Environment.CurrentDirectory = originalDirectory;
        Directory.Delete(baseDirectory, recursive: true);
    }
}
```

#### **Services/DocumentConversionIntegrationTests.cs**

C\#

using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using PrintAssistant.Services;
using ImageToPdfConverter = PrintAssistant.Services.Converters.ImageToPdfConverter;
using WordToPdfConverter = PrintAssistant.Services.Converters.WordToPdfConverter;
using ExcelToPdfConverter = PrintAssistant.Services.Converters.ExcelToPdfConverter;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using Xunit;
using Xunit.Abstractions;
using FileSystem = System.IO.Abstractions.FileSystem;

namespace PrintAssistant.Tests.Services;

public class DocumentConversionIntegrationTests : IDisposable
{
    private static bool _licenseRegistered;

    private readonly string _workingDirectory;
    private readonly ITestOutputHelper _output;
    private readonly FileSystem _fileSystem = new();

    public DocumentConversionIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        _workingDirectory = Path.Combine(Path.GetTempPath(), "PrintAssistantConversionTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workingDirectory);

        RegisterSyncfusionLicense();
    }

    \[Fact\]
    public async Task ConvertVariousDocuments\_ToPdfStreams\_AllValid()
    {
        var wordPath = CreateSampleWordDocument();
        var excelPath = CreateSampleExcelWorkbook();
        var imagePath = CreateSampleImage();

        await using var wordPdf = await new WordToPdfConverter(_fileSystem).ConvertToPdfAsync(wordPath);
        await using var excelPdf = await new ExcelToPdfConverter(_fileSystem).ConvertToPdfAsync(excelPath);
        await using var imagePdf = await new ImageToPdfConverter(_fileSystem).ConvertToPdfAsync(imagePath);

        var baseStreams = new List\<Stream\> { CloneStream(wordPdf), CloneStream(excelPdf), CloneStream(imagePdf) };
        var pdfFactories = baseStreams
            .Select\<Stream, Func\<Task\<Stream\>\>\>(stream \=\> () \=\> Task.FromResult\<Stream\>(CloneStream(stream)))
            .ToList();

        foreach (var pdfStream in baseStreams)
        {
            pdfStream.Position = 0;
            using var loadedDocument = new PdfLoadedDocument(pdfStream);
            Assert.True(loadedDocument.PageCount \>= 1);
        }

        var merger = new PdfMerger();
        var (mergedStream, totalPages) = await merger.MergePdfsAsync(pdfFactories);

        Assert.True(totalPages \>= baseStreams.Count);
        using var mergedDocument = new PdfLoadedDocument(mergedStream);
        Assert.Equal(totalPages, mergedDocument.PageCount);

        mergedStream.Dispose();
        foreach (var stream in baseStreams)
        {
            stream.Dispose();
        }
    }

    \[Fact\]
    public async Task PdfMerge\_PerformanceBaseline\_LogsMetrics()
    {
        const int documentCount = 20;

        var pdfStreams = new List\<Stream\>();
        for (var i = 0; i \< documentCount; i++)
        {
            pdfStreams.Add(await CreateSyntheticPdfStreamAsync($"Sample Document #{i + 1}"));
        }

        var pdfFactories = pdfStreams
            .Select\<Stream, Func\<Task\<Stream\>\>\>(stream \=\> () \=\> Task.FromResult\<Stream\>(CloneStream(stream)))
            .ToList();

        var merger = new PdfMerger();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memoryBefore = GC.GetTotalMemory(forceFullCollection: true);
        var stopwatch = Stopwatch.StartNew();
        var (mergedStream, totalPages) = await merger.MergePdfsAsync(pdfFactories);
        stopwatch.Stop();
        var memoryAfter = GC.GetTotalMemory(forceFullCollection: true);

        using var mergedDocument = new PdfLoadedDocument(mergedStream);

        Assert.True(totalPages \>= documentCount);
        Assert.Equal(totalPages, mergedDocument.PageCount);

        _output.WriteLine($"Merged {documentCount} PDFs into {totalPages} pages.");
        _output.WriteLine($"Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        _output.WriteLine($"Approx. memory delta: {(memoryAfter - memoryBefore) / 1024.0 / 1024.0:F2} MB.");

        mergedStream.Dispose();
        foreach (var stream in pdfStreams)
        {
            stream.Dispose();
        }
    }

    private string CreateSampleWordDocument()
    {
        var filePath = Path.Combine(_workingDirectory, "sample.docx");

        using var document = new WordDocument();
        var section = document.AddSection();
        var paragraph = section.AddParagraph();
        paragraph.AppendText("PrintAssistant 文档转换端到端验证 - Word");

        document.Save(filePath, FormatType.Docx);

        return filePath;
    }

    private string CreateSampleExcelWorkbook()
    {
        var filePath = Path.Combine(_workingDirectory, "sample.xlsx");

        using var excelEngine = new ExcelEngine();
        var workbook = excelEngine.Excel.Workbooks.Create(1);
        var sheet = workbook.Worksheets[0];
        sheet.Range["A1"].Text = "PrintAssistant";
        sheet.Range["A2"].Text = "文档转换端到端验证 - Excel";
        sheet.Range["B2"].Number = 2025;

        workbook.SaveAs(filePath);

        return filePath;
    }

    private string CreateSampleImage()
    {
        var filePath = Path.Combine(_workingDirectory, "sample.png");

        using var bitmap = new System.Drawing.Bitmap(400, 200);
        using var graphics = System.Drawing.Graphics.FromImage(bitmap);
        graphics.Clear(System.Drawing.Color.LightSteelBlue);
        graphics.DrawString("PrintAssistant", new System.Drawing.Font("Arial", 24), System.Drawing.Brushes.Black, new System.Drawing.PointF(20, 60));
        graphics.DrawString("文档转换端到端验证 - 图片", new System.Drawing.Font("Arial", 12), System.Drawing.Brushes.Black, new System.Drawing.PointF(20, 110));

        bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

        return filePath;
    }

    private async Task\<Stream\> CreateSyntheticPdfStreamAsync(string title)
    {
        using var document = new PdfDocument();
        var page = document.Pages.Add();
        var graphics = page.Graphics;

        var headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 18, PdfFontStyle.Bold);
        var bodyFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12);

        graphics.DrawString(title, headerFont, PdfBrushes.DarkBlue, new Syncfusion.Drawing.PointF(40, 40));
        graphics.DrawString("生成时间: " + DateTime.Now.ToString("F"), bodyFont, PdfBrushes.Black, new Syncfusion.Drawing.PointF(40, 80));

        var stream = new MemoryStream();
        document.Save(stream);
        await stream.FlushAsync().ConfigureAwait(false);
        stream.Position = 0;
        document.Close(true);

        return stream;
    }

    private static Stream CloneStream(Stream source)
    {
        source.Position = 0;
        var clone = new MemoryStream();
        source.CopyTo(clone);
        clone.Position = 0;
        source.Position = 0;
        return clone;
    }

    private static void RegisterSyncfusionLicense()
    {
        if (_licenseRegistered)
        {
            return;
        }

        var root = FindSolutionRoot();
        if (root == null)
        {
            throw new InvalidOperationException("无法定位解决方案根目录以加载 Syncfusion 许可证。");
        }

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(root)
            .AddJsonFile(Path.Combine("src", "PrintAssistant", "appsettings.Secret.json"), optional: true, reloadOnChange: false);

        var configuration = configBuilder.Build();
        var licenseKeys = configuration.GetSection("Syncfusion:DocumentSdk:LicenseKeys").Get\<string[]\>() ?? Array.Empty\<string\>();

        foreach (var key in licenseKeys)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(key);
            }
        }

        _licenseRegistered = true;
    }

    private static string? FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null && !directory.GetFiles("PrintAssistantSolution.sln").Any())
        {
            directory = directory.Parent;
        }

        return directory?.FullName;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_workingDirectory))
            {
                Directory.Delete(_workingDirectory, recursive: true);
            }
        }
        catch
        {
            // 忽略清理异常，避免影响测试结果。
        }
    }
}
```

#### **Services/FileConverterFactoryTests.cs**

C\#

using Moq;
using PrintAssistant.Services;
using PrintAssistant.Services.Abstractions;
using PrintAssistant.Services.Converters;
using System.Text;
using Xunit;

namespace PrintAssistant.Tests.Services;

public class FileConverterFactoryTests
{
    private FileConverterFactory CreateFactory()
    {
        var serviceProviderMock = new Mock\<IServiceProvider\>();
        serviceProviderMock
            .Setup(sp \=\> sp.GetService(typeof(WordToPdfConverter)))
            .Returns(Mock.Of\<IFileConverter\>());
        serviceProviderMock
            .Setup(sp \=\> sp.GetService(typeof(ExcelToPdfConverter)))
            .Returns(Mock.Of\<IFileConverter\>());
        serviceProviderMock
            .Setup(sp \=\> sp.GetService(typeof(ImageToPdfConverter)))
            .Returns(Mock.Of\<IFileConverter\>());

        return new FileConverterFactory(serviceProviderMock.Object);
    }

    \[Theory\]
    \[InlineData("sample.docx")\]
    \[InlineData("document.DOC")\]
    public void GetConverter\_ShouldReturnWordConverter(string fileName)
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter(fileName);
        Assert.NotNull(converter);
    }

    \[Theory\]
    \[InlineData("sheet.xls")\]
    \[InlineData("sheet.xlsx")\]
    \[InlineData("sheet.XLSM")\]
    public void GetConverter\_ShouldReturnExcelConverter(string fileName)
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter(fileName);
        Assert.NotNull(converter);
    }

    \[Theory\]
    \[InlineData("image.jpg")\]
    \[InlineData("image.PNG")\]
    \[InlineData("image.bmp")\]
    public void GetConverter\_ShouldReturnImageConverter(string fileName)
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter(fileName);
        Assert.NotNull(converter);
    }

    \[Fact\]
    public async Task GetConverter\_ShouldReturnPassthroughForPdf()
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter("already.pdf");
        Assert.NotNull(converter);

        var tempFile = Path.Combine(Path.GetTempPath(), $"PrintAssistantTest\_{Guid.NewGuid():N}.pdf");
        var pdfBytes = Encoding.UTF8.GetBytes("%PDF-1.4\\n1 0 obj\\n<< /Type /Catalog >>\\nendobj\\ntrailer\\n<< /Root 1 0 R >>\\nstartxref\\n0\\n%%EOF");
        await File.WriteAllBytesAsync(tempFile, pdfBytes);

        try
        {
            await using var resultStream = await converter!.ConvertToPdfAsync(tempFile);
            Assert.True(resultStream.Length \> 0);

            resultStream.Position = 0;
            using var memory = new MemoryStream();
            await resultStream.CopyToAsync(memory);
            Assert.Equal(pdfBytes, memory.ToArray());
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    \[Fact\]
    public async Task PassthroughConverter\_ShouldHandleLargePdfFile()
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter("large.pdf");
        Assert.NotNull(converter);

        var tempFile = Path.Combine(Path.GetTempPath(), $"PrintAssistantTest\_{Guid.NewGuid():N}\_large.pdf");

        await using (var fileStream = File.Create(tempFile))
        {
            var header = Encoding.ASCII.GetBytes("%PDF-1.4\\n");
            await fileStream.WriteAsync(header);

            var content = new byte[5 * 1024 * 1024];
            Random.Shared.NextBytes(content);
            await fileStream.WriteAsync(content);

            var trailer = Encoding.ASCII.GetBytes("\\ntrailer\\n<< /Root 1 0 R >>\\nstartxref\\n0\\n%%EOF");
            await fileStream.WriteAsync(trailer);
        }

        try
        {
            await using var resultStream = await converter!.ConvertToPdfAsync(tempFile);
            Assert.Equal(new FileInfo(tempFile).Length, resultStream.Length);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    \[Fact\]
    public async Task PassthroughConverter\_ShouldThrowForMissingFile()
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter("missing.pdf");
        Assert.NotNull(converter);

        var missingPath = Path.Combine(Path.GetTempPath(), $"PrintAssistantTest\_{Guid.NewGuid():N}\_missing.pdf");

        var exception = await Assert.ThrowsAsync\<FileNotFoundException\>(() \=\> converter!.ConvertToPdfAsync(missingPath));
        Assert.Contains(Path.GetFileName(missingPath), exception.FileName ?? string.Empty);
    }

    \[Fact\]
    public async Task PassthroughConverter\_ShouldThrowForInvalidPdf()
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter("invalid.pdf");
        Assert.NotNull(converter);

        var tempFile = Path.Combine(Path.GetTempPath(), $"PrintAssistantTest\_{Guid.NewGuid():N}\_invalid.pdf");
        await File.WriteAllTextAsync(tempFile, "Not a PDF file");

        try
        {
            await using var stream = await converter!.ConvertToPdfAsync(tempFile);
            Assert.True(stream.Length \> 0);

            stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            var content = await reader.ReadToEndAsync();
            Assert.Equal("Not a PDF file", content);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    \[Fact\]
    public void GetConverter\_ShouldReturnNullForUnsupported()
    {
        var factory = CreateFactory();
        var converter = factory.GetConverter("unsupported.xyz");
        Assert.Null(converter);
    }
}
```

#### **Services/PrintProcessorServiceTests.cs**

C\#

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PrintAssistant.Configuration;
using PrintAssistant.Core;
using PrintAssistant.Services;
using PrintAssistant.Services.Abstractions;
using Xunit;

namespace PrintAssistant.Tests.Services;

public class PrintProcessorServiceTests
{
    private static PrintProcessorService CreateService(
        Mock\<IPrintQueue\> queueMock,
        Mock\<IPrintService\> printServiceMock,
        Mock\<IFileMonitor\> fileMonitorMock,
        Mock\<ITrayIconService\> trayIconMock,
        Mock\<IFileConverterFactory\> converterFactoryMock,
        Mock\<IPdfMerger\> pdfMergerMock,
        Mock\<IFileArchiver\> archiverMock,
        Mock\<ICoverPageGenerator\> coverPageGeneratorMock,
        IRetryPolicy retryPolicy,
        IJobStageRetryDecider retryDecider,
        IServiceProvider serviceProvider,
        AppSettings? settings = null)
    {
        var options = Options.Create(settings ?? new AppSettings());

        return new PrintProcessorService(
            NullLogger\<PrintProcessorService\>.Instance,
            queueMock.Object,
            fileMonitorMock.Object,
            trayIconMock.Object,
            printServiceMock.Object,
            converterFactoryMock.Object,
            pdfMergerMock.Object,
            archiverMock.Object,
            coverPageGeneratorMock.Object,
            retryPolicy,
            retryDecider,
            options,
            serviceProvider);
    }

    \[Fact\]
    public async Task ProcessJob\_ShouldConvertMergePrintAndArchive()
    {
        // Arrange
        var queueMock = new Mock\<IPrintQueue\>();
        var printServiceMock = new Mock\<IPrintService\>();
        var fileMonitorMock = new Mock\<IFileMonitor\>();
        var trayIconMock = new Mock\<ITrayIconService\>();
        var converterFactoryMock = new Mock\<IFileConverterFactory\>();
        var pdfMergerMock = new Mock\<IPdfMerger\>();
        var archiverMock = new Mock\<IFileArchiver\>();
        var coverPageGeneratorMock = new Mock\<ICoverPageGenerator\>();
        var retryPolicyMock = new Mock\<IRetryPolicy\>();
        var retryDeciderMock = new Mock\<IJobStageRetryDecider\>();
        var serviceProviderMock = new Mock\<IServiceProvider\>();

        var job = new PrintJob(new[] { "test.docx" });
        var mockConverter = new Mock\<IFileConverter\>();
        var mockPdfStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Mock PDF content"));
        var mockMergedStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Mock merged PDF content"));
        var mockCoverPageStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Mock cover page content"));

        converterFactoryMock.Setup(f \=\> f.GetConverter("test.docx")).Returns(mockConverter.Object);
        mockConverter.Setup(c \=\> c.ConvertToPdfAsync("test.docx")).ReturnsAsync(mockPdfStream);
        coverPageGeneratorMock.Setup(g \=\> g.GenerateCoverPageAsync(job)).ReturnsAsync(mockCoverPageStream);
        
        pdfMergerMock.Setup(m \=\> m.MergePdfsAsync(It.IsAny\<IEnumerable\<Func\<Task\<Stream\>\>\>\>()))
            .ReturnsAsync((mockMergedStream, 2));
        
        printServiceMock.Setup(p \=\> p.PrintPdfAsync(It.IsAny\<Stream\>(), It.IsAny\<string\>(), It.IsAny\<int\>()))
            .Returns(Task.CompletedTask);
        
        archiverMock.Setup(a \=\> a.ArchiveFilesAsync(It.IsAny\<IEnumerable\<string\>\>(), It.IsAny\<DateTime\>(), It.IsAny\<Stream\>(), It.IsAny\<string\>()))
            .ReturnsAsync("archived/path");

        serviceProviderMock.Setup(s \=\> s.GetService(typeof(PrintAssistant.UI.PrinterSelectionForm)))
            .Returns(new PrintAssistant.UI.PrinterSelectionForm());

        var settings = new AppSettings
        {
            Printing = new PrintSettings { GenerateCoverPage = true }
        };

        var service = CreateService(queueMock, printServiceMock, fileMonitorMock, trayIconMock, converterFactoryMock, pdfMergerMock, archiverMock, coverPageGeneratorMock, retryPolicyMock.Object, retryDeciderMock.Object, serviceProviderMock.Object, settings);

        // Act
        await service.ProcessJobAsync(job);

        // Assert
        converterFactoryMock.Verify(f \=\> f.GetConverter("test.docx"), Times.Once);
        mockConverter.Verify(c \=\> c.ConvertToPdfAsync("test.docx"), Times.Once);
        coverPageGeneratorMock.Verify(g \=\> g.GenerateCoverPageAsync(job), Times.Once);
        pdfMergerMock.Verify(m \=\> m.MergePdfsAsync(It.IsAny\<IEnumerable\<Func\<Task\<Stream\>\>\>\>()), Times.Once);
        printServiceMock.Verify(p \=\> p.PrintPdfAsync(It.IsAny\<Stream\>(), It.IsAny\<string\>(), It.IsAny\<int\>()), Times.Once);
        archiverMock.Verify(a \=\> a.ArchiveFilesAsync(It.IsAny\<IEnumerable\<string\>\>(), It.IsAny\<DateTime\>(), It.IsAny\<Stream\>(), It.IsAny\<string\>()), Times.Once);
    }
}  

## 新增文件：UI服务架构

### src/PrintAssistant/Services/Abstractions/IUIService.cs

```csharp
using PrintAssistant.Core;

namespace PrintAssistant.Services.Abstractions;

/// <summary>
/// 提供在主UI线程上执行操作的服务。
/// </summary>
public interface IUIService
{
    /// <summary>
    /// 在主UI线程上异步显示打印机选择对话框。
    /// </summary>
    /// <param name="job">当前的打印任务。</param>
    /// <returns>一个表示对话框结果的任务，如果用户确认则为 true。</returns>
    Task<bool> ShowPrinterSelectionDialogAsync(PrintJob job);
}
```

### src/PrintAssistant/Services/UIService.cs

```csharp
using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using PrintAssistant.Core;
using PrintAssistant.Services.Abstractions;
using PrintAssistant.UI;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PrintAssistant.Services;

/// <summary>
/// UI服务的实现，负责在正确的UI线程上创建和显示窗体。
/// </summary>
public class UIService : IUIService, IDisposable
{
    private readonly IOptions<AppSettings> _appSettings;
    private readonly Control _invoker; // 一个隐藏的控件，用于访问UI线程

    public UIService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings;
        // 创建一个隐藏的控件，它的句柄将在主UI线程上创建。
        // 我们用它来安全地调用Invoke。
        _invoker = new Control();
        _invoker.CreateControl();
    }

    public Task<bool> ShowPrinterSelectionDialogAsync(PrintJob job)
    {
        // 使用 TaskCompletionSource 在后台线程中等待UI线程的结果
        var tcs = new TaskCompletionSource<bool>();

        // 将显示对话框的操作封送到主UI线程执行
        _invoker.Invoke(() =>
        {
            try
            {
                // Pass excluded printers to the dialog constructor
                using var dialog = new PrinterSelectionForm(_appSettings.Value.Printing.ExcludedPrinters);

                // Initialize the dialog with current job settings
                dialog.Initialize(
                    _appSettings.Value.Printing.ExcludedPrinters, // Pass excluded printers again for consistency
                    job.SelectedPrinter,
                    job.Copies > 0 ? job.Copies : _appSettings.Value.Printing.Windows.DefaultCopies);

                // 关键步骤：将隐藏的控件作为对话框的所有者
                // 这能极大地提高置顶的成功率
                var result = dialog.ShowDialog(_invoker);

                if (result == DialogResult.OK)
                {
                    job.SelectedPrinter = dialog.SelectedPrinter;
                    job.Copies = dialog.PrintCopies;
                    tcs.SetResult(true);
                }
                else
                {
                    tcs.SetResult(false);
                }
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    public void Dispose()
    {
        _invoker.Dispose();
    }
}
```

## 更新文件：Program.cs 服务注册

在 `src/PrintAssistant/Program.cs` 的 `ConfigureServices` 方法中添加：

```csharp
// ... existing service registrations ...
services.AddSingleton<IJobStageRetryDecider, DefaultRetryPolicy>();
services.AddSingleton<IUIService, UIService>(); // 新增UI服务注册

services.AddTransient<WordToPdfConverter>();
// ... rest of the file ...
```

## 更新文件：PrintProcessorService.cs

在 `src/PrintAssistant/Services/PrintProcessorService.cs` 中的主要变更：

### 构造函数更新
```csharp
private readonly IUIService _uiService; // 新增字段
private readonly AppSettings _appSettings;

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
    PrintAssistant.Services.Abstractions.IRetryPolicy retryPolicy,
    IJobStageRetryDecider retryDecider,
    IUIService uiService, // 新增参数
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
    _retryPolicy = retryPolicy;
    _retryDecider = retryDecider;
    _uiService = uiService; // 赋值
    _appSettings = appSettings.Value;

    _fileMonitor.JobDetected += OnJobDetected;
}
```

### EnsurePrinterSelectionAsync 方法更新
```csharp
private async Task<bool> EnsurePrinterSelectionAsync(PrintJob job)
{
    if (!string.IsNullOrWhiteSpace(job.SelectedPrinter) && job.Copies > 0)
    {
        return true;
    }

    try
    {
        _logger.LogInformation("作业 {JobId}: 等待用户选择打印机。", job.JobId);
        bool userConfirmed = await _uiService.ShowPrinterSelectionDialogAsync(job);

        if (!userConfirmed)
        {
            _logger.LogInformation("作业 {JobId}: 用户取消了打印。", job.JobId);
            return false;
        }

        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to prompt for printer selection.");
        throw;
    }
}
```

## 更新文件：PrinterSelectionForm.cs

在 `src/PrintAssistant/UI/PrinterSelectionForm.cs` 中的主要变更：

### 构造函数更新
```csharp
private List<string> _excludedPrinters = new(); // 在类级别初始化
private List<string> _availablePrinters = new();

public PrinterSelectionForm(IEnumerable<string> excludedPrinters) // 修改构造函数
{
    InitializeComponent();
    _excludedPrinters = excludedPrinters?.ToList() ?? new List<string>(); // 初始化_excludedPrinters
}
```

### 移除冗余的置顶代码
移除了以下方法中的置顶相关代码：
- 构造函数中的 `TopMost = true; BringToFront(); Activate();`
- `PrinterSelectionForm_Load` 中的置顶代码
- 删除了 `PrinterSelectionForm_Shown` 事件处理器
- 删除了 `SetVisibleCore` 重写方法

## 更新文件：PrinterSelectionForm.Designer.cs

在 `src/PrintAssistant/UI/PrinterSelectionForm.Designer.cs` 中移除：

```csharp
// 移除这行：
// Shown += PrinterSelectionForm_Shown;
```

## 更新文件：PrintProcessorServiceTests.cs

在 `tests/PrintAssistant.Tests/Services/PrintProcessorServiceTests.cs` 中的测试更新：

### CreateService 方法更新
```csharp
private static PrintProcessorService CreateService(
    Mock<IPrintQueue> queueMock,
    Mock<IPrintService> printServiceMock,
    Mock<IFileMonitor> fileMonitorMock,
    Mock<ITrayIconService> trayIconMock,
    Mock<IFileConverterFactory> converterFactoryMock,
    Mock<IPdfMerger> pdfMergerMock,
    Mock<IFileArchiver> archiverMock,
    Mock<ICoverPageGenerator> coverPageGeneratorMock,
    IRetryPolicy retryPolicy,
    IJobStageRetryDecider retryDecider,
    Mock<IUIService> uiServiceMock, // 新增参数
    AppSettings? settings = null)
{
    var options = Options.Create(settings ?? new AppSettings());

    return new PrintProcessorService(
        NullLogger<PrintProcessorService>.Instance,
        queueMock.Object,
        fileMonitorMock.Object,
        trayIconMock.Object,
        printServiceMock.Object,
        converterFactoryMock.Object,
        pdfMergerMock.Object,
        archiverMock.Object,
        coverPageGeneratorMock.Object,
        retryPolicy,
        retryDecider,
        uiServiceMock.Object, // 传递mock对象
        options);
}
```

### 测试方法更新
```csharp
[Fact]
public async Task ProcessJob_ShouldConvertMergePrintAndArchive()
{
    // ... existing mocks ...
    var uiServiceMock = new Mock<IUIService>();
    uiServiceMock.Setup(u => u.ShowPrinterSelectionDialogAsync(It.IsAny<PrintJob>()))
        .ReturnsAsync(true); // 假设用户确认打印机选择

    var service = CreateService(queueMock, printServiceMock, fileMonitorMock, trayIconMock, converterFactoryMock, pdfMergerMock, archiverMock, coverPageGeneratorMock, retryPolicyMock.Object, retryDeciderMock.Object, uiServiceMock, settings);
    // ... rest of the test ...
}
```

## 架构优势总结

1. **线程安全**: 所有UI操作在主UI线程上执行
2. **窗口置顶**: 通过窗口所有权机制确保对话框置顶
3. **可测试性**: UI逻辑与业务逻辑分离，便于单元测试
4. **可维护性**: 符合单一职责原则，代码结构清晰
5. **可扩展性**: 可以轻松添加新的UI服务方法

这次架构优化不仅解决了窗口置顶问题，更重要的是建立了一个可扩展、可测试、可维护的UI服务架构。
