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
│       │   └── print_icon.ico  
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

### **1. 源代码 (src/PrintAssistant/)**

#### **PrintAssistant.csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>  
    <OutputType>WinExe</OutputType>  
    <TargetFramework>net8.0-windows</TargetFramework>  
    <Nullable>enable</Nullable>  
    <UseWindowsForms>true</UseWindowsForms>  
    <ImplicitUsings>enable</ImplicitUsings>  
    <ApplicationIcon>Assets\print_icon.ico</ApplicationIcon>  
  </PropertyGroup>

  <ItemGroup>  
    <Content Include="Assets\print_icon.ico" />  
  </ItemGroup>

  <ItemGroup>  
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />  
    <PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />  
    <PackageReference Include="Serilog.Settings.Configuration" Version="8.0.0" />  
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />  
    <PackageReference Include="System.IO.Abstractions" Version="20.0.15" />  
    <PackageReference Include="System.Threading.Tasks.Dataflow" Version="8.0.0" />  
    <PackageReference Include="Syncfusion.DocIO.Net.Core" Version="*" />  
    <PackageReference Include="Syncfusion.DocIORenderer.Net.Core" Version="*" />  
    <PackageReference Include="Syncfusion.Pdf.Net.Core" Version="*" />  
    <PackageReference Include="Syncfusion.PdfToImageConverter.Net" Version="*" />  
    <PackageReference Include="Syncfusion.XlsIO.Net.Core" Version="*" />  
    <PackageReference Include="Syncfusion.XlsIORenderer.Net.Core" Version="*" />  
  </ItemGroup>

  <ItemGroup>  
    <None Update="appsettings.json">  
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>  
    </None>  
  </ItemGroup>

</Project>
```

#### **appsettings.json**

```json
{  
  "Serilog": {  
    "MinimumLevel": {  
      "Default": "Information",  
      "Override": {  
        "Microsoft": "Warning",  
        "System": "Warning"  
      }  
    },  
    "WriteTo": [  
      {  
        "Name": "File",  
        "Args": {  
          "path": "Logs/log-.txt",  
          "rollingInterval": "Day",  
          "retainedFileTimeLimit": "7.00:00:00"  
        }  
      }  
    ]  
  },  
  "AppSettings": {  
    "Monitoring": {  
      "Path": "",  
      "DebounceIntervalMilliseconds": 2500,  
      "MaxFileSizeMegaBytes": 100  
    },  
    "UnsupportedFiles": {  
      "MoveToPath": ""  
    },  
    "Printing": {  
      "ExcludedPrinters": [],  
      "GenerateCoverPage": true  
    },  
    "Archiving": {  
      "SubdirectoryFormat": "Processed_{0:yyyyMMdd_HHmmss}"  
    },  
    "Logging": {  
      "Path": ""  
    }  
  }  
}
```

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
using PrintAssistant.Services.Converters;
using PrintAssistant.UI;
using Serilog;
using System.IO.Abstractions;

namespace PrintAssistant;

/// <summary>
/// 定义应用程序入口点并配置通用主机构建流程。
/// </summary>
internal static class Program
{
    /// <summary>
    /// 应用程序的主入口点。
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_SYNCFUSION_LICENSE_KEY");

        ApplicationConfiguration.Initialize();

        var builder = Host.CreateApplicationBuilder(args);

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
        services.AddSingleton<IFileMonitor, FileMonitorService>();
        services.AddSingleton<ITrayIconService, TrayIconService>();
        services.AddHostedService<PrintProcessorService>();

        services.AddSingleton<IFileConverterFactory, FileConverterFactory>();
        services.AddSingleton<IPrintService, PrintService>();
        services.AddSingleton<IFileArchiver, FileArchiver>();
        services.AddSingleton<IPdfMerger, PdfMerger>();

        services.AddTransient<WordToPdfConverter>();
        services.AddTransient<ExcelToPdfConverter>();
        services.AddTransient<ImageToPdfConverter>();
        services.AddTransient<ICoverPageGenerator, CoverPageGenerator>();

        services.AddTransient<SettingsForm>();
        services.AddTransient<PrinterSelectionForm>();
    }
}
```

... (rest of document identical to new copy) ...

