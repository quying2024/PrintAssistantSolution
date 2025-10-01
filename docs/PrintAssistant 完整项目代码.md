

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
    \<PackageReference Include\="Syncfusion.PdfToImageConverter.Net.Core" Version\="\*" /\>  
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

C\#

using Microsoft.Extensions.DependencyInjection;  
using Microsoft.Extensions.Hosting;  
using PrintAssistant.Configuration;  
using PrintAssistant.Services;  
using PrintAssistant.Services.Abstractions;  
using PrintAssistant.Services.Converters;  
using PrintAssistant.UI;  
using Serilog;  
using System.IO.Abstractions;

namespace PrintAssistant;

static class Program  
{  
    /// \<summary\>  
    ///  应用程序的主入口点。  
    /// \</summary\>  
     
    static void Main(string args)  
    {  
        // \=================================================================================  
        // 重要：请在此处填入您的 Syncfusion 社区许可密钥  
        // Get your key from https://www.syncfusion.com/products/communitylicense  
        // \=================================================================================  
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR\_SYNCFUSION\_LICENSE\_KEY");

        ApplicationConfiguration.Initialize();

        var builder \= Host.CreateApplicationBuilder(args);

        // 配置 Serilog 日志记录器  
        builder.Services.AddSerilog((services, lc) \=\> lc  
           .ReadFrom.Configuration(builder.Configuration));

        // 配置和注册所有服务  
        ConfigureServices(builder.Services, builder.Configuration);

        var host \= builder.Build();

        // 启动应用程序的托管服务，这将初始化托盘图标和后台任务  
        // Application.Run() 会阻塞主线程，直到最后一个窗体关闭，  
        // 这对于无主窗体的托盘应用是理想的。  
        // 我们的 IHostedService 将管理应用的实际生命周期。  
        \_ \= host.Services.GetRequiredService\<ITrayIconService\>(); // 确保托盘服务被创建

        // 启动主机，这将调用所有IHostedService的StartAsync方法  
        host.Start();

        Application.Run();

        // 当Application.Exit()被调用时，优雅地停止主机  
        host.StopAsync().GetAwaiter().GetResult();  
    }

    /// \<summary\>  
    /// 配置所有服务的依赖注入。  
    /// \</summary\>  
    /// \<param name="services"\>服务集合\</param\>  
    /// \<param name="configuration"\>应用程序配置\</param\>  
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)  
    {  
        // 注册强类型配置选项  
        services.Configure\<AppSettings\>(configuration.GetSection("AppSettings"));

        // 注册文件系统抽象，使其可在单元测试中被模拟  
        services.AddSingleton\<IFileSystem, FileSystem\>();

        // 注册核心后台服务  
        // IPrintQueue 作为单例，确保整个应用只有一个打印队列  
        services.AddSingleton\<IPrintQueue, PrintQueueService\>();  
        // IFileMonitor 作为单例，确保只有一个文件系统监视器实例  
        services.AddSingleton\<IFileMonitor, FileMonitorService\>();  
        // ITrayIconService 作为单例，管理唯一的系统托盘图标  
        services.AddSingleton\<ITrayIconService, TrayIconService\>();  
        // PrintProcessorService 是一个托管服务，负责消费打印队列，由通用主机管理其生命周期  
        services.AddHostedService\<PrintProcessorService\>();

        // 注册功能性服务  
        services.AddSingleton\<IFileConverterFactory, FileConverterFactory\>();  
        services.AddSingleton\<IPrintService, PrintService\>();  
        services.AddSingleton\<IFileArchiver, FileArchiver\>();  
        services.AddSingleton\<IPdfMerger, PdfMerger\>();  
          
        // 转换器和封面页生成器是无状态的，注册为瞬态  
        services.AddTransient\<WordToPdfConverter\>();  
        services.AddTransient\<ExcelToPdfConverter\>();  
        services.AddTransient\<ImageToPdfConverter\>();  
        services.AddTransient\<ICoverPageGenerator, CoverPageGenerator\>();

        // 注册UI窗体为瞬态，每次请求时都创建新实例  
        services.AddTransient\<SettingsForm\>();  
        services.AddTransient\<PrinterSelectionForm\>();  
    }  
}

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
public interface IFileArchiver { Task ArchiveFilesAsync(IEnumerable\<string\> sourceFiles, DateTime jobCreationTime); void MoveUnsupportedFile(string sourceFile); }

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

using PrintAssistant.Core;  
using PrintAssistant.Services.Abstractions;  
using Syncfusion.Drawing;  
using Syncfusion.Pdf;  
using Syncfusion.Pdf.Graphics;  
using Syncfusion.Pdf.Grid;

namespace PrintAssistant.Services;

/// \<summary\>  
/// 负责为打印任务生成一个包含元数据信息的封面页。  
/// \</summary\>  
public class CoverPageGenerator : ICoverPageGenerator  
{  
    public Task\<Stream\> GenerateCoverPageAsync(PrintJob job)  
    {  
        using var document \= new PdfDocument();  
        var page \= document.Pages.Add();  
        var graphics \= page.Graphics;  
        var font \= new PdfStandardFont(PdfFontFamily.Helvetica, 12);  
        var titleFont \= new PdfStandardFont(PdfFontFamily.Helvetica, 18, PdfFontStyle.Bold);

        float yPos \= 20;

        // 绘制标题  
        graphics.DrawString("打印任务清单", titleFont, PdfBrushes.Black, new PointF(20, yPos));  
        yPos \+= 40;

        // 绘制任务元数据  
        graphics.DrawString($"任务ID: {job.JobId}", font, PdfBrushes.Black, new PointF(20, yPos));  
        yPos \+= 20;  
        graphics.DrawString($"打印时间: {job.CreationTime:yyyy-MM-dd HH:mm:ss}", font, PdfBrushes.Black, new PointF(20, yPos));  
        yPos \+= 40;

        // 创建一个表格来显示文件列表  
        var pdfGrid \= new PdfGrid();  
        pdfGrid.Columns.Add(2);  
        pdfGrid.Headers.Add(1);

        var header \= pdfGrid.Headers;  
        header.Cells.Value \= "文件名";  
        header.Cells.\[1\]Value \= "页数 (估算)";  
        header.Style.Font \= new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold);

        foreach (var file in job.SourceFilePaths)  
        {  
            var row \= pdfGrid.Rows.Add();  
            row.Cells.Value \= Path.GetFileName(file);  
            row.Cells.\[1\]Value \= "N/A"; // 页数在合并后才精确知道  
        }

        // 绘制表格  
        pdfGrid.Draw(page, new PointF(20, yPos));

        var stream \= new MemoryStream();  
        document.Save(stream);  
        stream.Position \= 0;

        return Task.FromResult\<Stream\>(stream);  
    }  
}

#### **Services/FileArchiver.cs**

C\#

using Microsoft.Extensions.Options;  
using PrintAssistant.Configuration;  
using PrintAssistant.Services.Abstractions;  
using System.IO.Abstractions;

namespace PrintAssistant.Services;

public class FileArchiver(IFileSystem fileSystem, IOptions\<AppSettings\> appSettings) : IFileArchiver  
{  
    private readonly IFileSystem \_fileSystem \= fileSystem;  
    private readonly AppSettings \_settings \= appSettings.Value;

    public Task ArchiveFilesAsync(IEnumerable\<string\> sourceFiles, DateTime jobCreationTime)  
    {  
        var monitorPath \= GetMonitorPath();  
        var archiveSubDirName \= string.Format(\_settings.Archiving.SubdirectoryFormat, jobCreationTime);  
        var archivePath \= \_fileSystem.Path.Combine(monitorPath, archiveSubDirName);

        \_fileSystem.Directory.CreateDirectory(archivePath);

        foreach (var sourceFile in sourceFiles)  
        {  
            if (\_fileSystem.File.Exists(sourceFile))  
            {  
                var destFileName \= \_fileSystem.Path.Combine(archivePath, \_fileSystem.Path.GetFileName(sourceFile));  
                \_fileSystem.File.Move(sourceFile, destFileName);  
            }  
        }  
        return Task.CompletedTask;  
    }

    public void MoveUnsupportedFile(string sourceFile)  
    {  
        var unsupportedPath \= GetUnsupportedFilesPath();  
        \_fileSystem.Directory.CreateDirectory(unsupportedPath);

        if (\_fileSystem.File.Exists(sourceFile))  
        {  
            var destFileName \= \_fileSystem.Path.Combine(unsupportedPath, \_fileSystem.Path.GetFileName(sourceFile));  
            \_fileSystem.File.Move(sourceFile, destFileName);  
        }  
    }

    private string GetMonitorPath()  
    {  
        return\!string.IsNullOrEmpty(\_settings.Monitoring.Path)  
           ? \_settings.Monitoring.Path  
            : \_fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PrintJobs");  
    }

    private string GetUnsupportedFilesPath()  
    {  
        return\!string.IsNullOrEmpty(\_settings.UnsupportedFiles.MoveToPath)  
           ? \_settings.UnsupportedFiles.MoveToPath  
            : \_fileSystem.Path.Combine(GetMonitorPath(), "Unsupported");  
    }  
}

#### **Services/FileConverterFactory.cs**

C\#

using PrintAssistant.Services.Abstractions;  
using PrintAssistant.Services.Converters;

namespace PrintAssistant.Services;

public class FileConverterFactory(IServiceProvider serviceProvider) : IFileConverterFactory  
{  
    private readonly IServiceProvider \_serviceProvider \= serviceProvider;

    public IFileConverter? GetConverter(string filePath)  
    {  
        var extension \= Path.GetExtension(filePath).ToLowerInvariant();  
        return extension switch  
        {  
            ".doc" or ".docx" \=\> \_serviceProvider.GetService(typeof(WordToPdfConverter)) as IFileConverter,  
            ".xlsx" \=\> \_serviceProvider.GetService(typeof(ExcelToPdfConverter)) as IFileConverter,  
            ".jpg" or ".jpeg" or ".png" or ".bmp" \=\> \_serviceProvider.GetService(typeof(ImageToPdfConverter)) as IFileConverter,  
            ".pdf" \=\> new PassthroughConverter(), // 对于PDF文件，我们不需要转换  
            \_ \=\> null,  
        };  
    }

    /// \<summary\>  
    /// 一个特殊的“转换器”，用于处理已经是PDF的文件。它只是简单地将源文件流复制出来。  
    /// \</summary\>  
    private class PassthroughConverter : IFileConverter  
    {  
        public async Task\<Stream\> ConvertToPdfAsync(string sourceFilePath)  
        {  
            var memoryStream \= new MemoryStream();  
            await using (var fileStream \= new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))  
            {  
                await fileStream.CopyToAsync(memoryStream);  
            }  
            memoryStream.Position \= 0;  
            return memoryStream;  
        }  
    }  
}

#### **Services/FileMonitorService.cs**

C\#

using Microsoft.Extensions.Logging;  
using Microsoft.Extensions.Options;  
using PrintAssistant.Configuration;  
using PrintAssistant.Core;  
using PrintAssistant.Services.Abstractions;  
using System.Collections.Concurrent;  
using System.IO.Abstractions;  
using Timer \= System.Timers.Timer;

namespace PrintAssistant.Services;

public class FileMonitorService : IFileMonitor, IDisposable  
{  
    //... (实现代码)  
}

*(由于代码过长，此处省略 FileMonitorService.cs 的完整实现，但它将包含 FileSystemWatcher、防抖计时器和文件队列的逻辑。)*

#### **Services/PdfMerger.cs**

C\#

using PrintAssistant.Services.Abstractions;  
using Syncfusion.Pdf;  
using Syncfusion.Pdf.Parsing;

namespace PrintAssistant.Services;

public class PdfMerger : IPdfMerger  
{  
    public Task\<(Stream MergedPdfStream, int TotalPages)\> MergePdfsAsync(IEnumerable\<Stream\> pdfStreams)  
    {  
        using var finalDocument \= new PdfDocument();  
          
        // 使用 Syncfusion 的 Merge 方法合并所有流  
        PdfDocumentBase.Merge(finalDocument, pdfStreams.ToArray());

        var mergedStream \= new MemoryStream();  
        finalDocument.Save(mergedStream);  
        mergedStream.Position \= 0;

        int totalPages \= finalDocument.Pages.Count;

        return Task.FromResult\<(Stream, int)\>((mergedStream, totalPages));  
    }  
}

#### **Services/PrintProcessorService.cs**

C\#

using Microsoft.Extensions.Hosting;  
using Microsoft.Extensions.Logging;  
using Microsoft.Extensions.Options;  
using PrintAssistant.Configuration;  
using PrintAssistant.Core;  
using PrintAssistant.Services.Abstractions;  
using PrintAssistant.UI;  
using System.IO.Abstractions;

namespace PrintAssistant.Services;

public class PrintProcessorService : BackgroundService  
{  
    //... (实现代码)  
}

*(由于代码过长，此处省略 PrintProcessorService.cs 的完整实现，但它将包含消费队列、调用转换、合并、打印、归档等核心处理管道逻辑。)*

#### **Services/PrintQueueService.cs**

C\#

using PrintAssistant.Core;  
using PrintAssistant.Services.Abstractions;  
using System.Threading.Tasks.Dataflow;

namespace PrintAssistant.Services;

public class PrintQueueService : IPrintQueue  
{  
    private readonly BufferBlock\<PrintJob\> \_queue \= new();

    public Task EnqueueJobAsync(PrintJob job) \=\> \_queue.SendAsync(job).AsTask();

    public Task\<PrintJob\> DequeueJobAsync(CancellationToken cancellationToken) \=\> \_queue.ReceiveAsync(cancellationToken);

    public IReceivableSourceBlock\<PrintJob\> AsReceivableSourceBlock() \=\> \_queue;  
}

#### **Services/PrintService.cs**

C\#

using PrintAssistant.Services.Abstractions;  
using Syncfusion.PdfToImageConverter;  
using System.Drawing.Printing;

namespace PrintAssistant.Services;

public class PrintService : IPrintService  
{  
    public Task PrintPdfAsync(Stream pdfStream, string printerName, int copies)  
    {  
        //... (实现代码)  
}

*(由于代码过长，此处省略 PrintService.cs 的完整实现，但它将包含使用 PdfToImageConverter 和 PrintDocument 进行打印的逻辑。)*

#### **Services/TrayIconService.cs**

C\#

using Microsoft.Extensions.DependencyInjection;  
using Microsoft.Extensions.Hosting;  
using Microsoft.Extensions.Logging;  
using PrintAssistant.Core;  
using PrintAssistant.Services.Abstractions;  
using PrintAssistant.UI;

namespace PrintAssistant.Services;

public class TrayIconService : ITrayIconService, IDisposable  
{  
    //... (实现代码)  
}

*(由于代码过长，此处省略 TrayIconService.cs 的完整实现，但它将包含 NotifyIcon 的创建、菜单管理、跨线程更新Tooltip等逻辑。)*

#### **UI/ (所有窗体文件)**

*(由于UI代码（尤其是 Designer.cs 文件）非常冗长且由IDE自动生成，此处仅提供关键逻辑部分的伪代码。实际项目中，您可以使用Visual Studio的窗体设计器来创建这些界面。)*

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
    \<TargetFramework\>net8.0\</TargetFramework\>  
    \<ImplicitUsings\>enable\</ImplicitUsings\>  
    \<Nullable\>enable\</Nullable\>  
    \<IsPackable\>false\</IsPackable\>  
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
