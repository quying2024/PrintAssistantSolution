using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PrintAssistant.Configuration;
using PrintAssistant.Services;
using PrintAssistant.Services.Abstractions;
using PrintAssistant.Services.Converters;
using PrintAssistant.Services.Retry;
using PrintAssistant.UI;
using Serilog;
using System.Linq;
using System.IO.Abstractions;

namespace PrintAssistant;

/// <summary>
/// 定义应用程序入口点并配置通用主机构建流程。
/// </summary>
public static class Program
{
    /// <summary>
    /// 应用程序的主入口点。
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        // 尝试加载 appsettings.Secret.json，以便从中读取 Syncfusion 许可证
        builder.Configuration.AddJsonFile("appsettings.Secret.json", optional: true, reloadOnChange: true);
        RegisterSyncfusionLicense(builder.Configuration);

        var serilogPaths = NormalizeSerilogFilePaths(builder.Configuration);
        EnsureSerilogDirectories(serilogPaths);

        Console.WriteLine("Serilog file sinks:");
        if (serilogPaths.Count == 0)
        {
            Console.WriteLine("<no file sinks configured>");
        }
        else
        {
            foreach (var path in serilogPaths)
            {
                Console.WriteLine(path);
            }
        }

        // 配置 Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();
        Log.Information("PrintAssistant host initializing.");

        builder.Logging.AddSerilog(dispose: true);

        ConfigureServices(builder.Services, builder.Configuration);

        using var host = builder.Build();

        // 确保托盘图标服务被实例化以注册通知图标
        _ = host.Services.GetRequiredService<ITrayIconService>();

        host.Start();
        Log.Information("PrintAssistant host started.");

        Application.Run();

        host.StopAsync().GetAwaiter().GetResult();
        Log.Information("PrintAssistant host stopped.");
        Log.CloseAndFlush();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // 配置强类型选项
        services.Configure<AppSettings>(configuration.GetSection("ApplicationSettings"));

        // 文件系统抽象
        services.AddSingleton<IFileSystem, FileSystem>();

        // 核心服务注册
        services.AddSingleton<IPrintQueue, PrintQueueService>();
        services.AddSingleton<IFileArchiver, FileArchiver>();
        services.AddSingleton<IFileMonitor, FileMonitorService>();
        services.AddSingleton<ITrayIconService, TrayIconService>();
        bool useMock = configuration.GetValue<bool>("ApplicationSettings:Printing:UseMockPrintService");
        if (useMock)
        {
            services.AddSingleton<IPrintService, MockPrintService>();
        }
        else
        {
            services.AddSingleton<IPrintService, WindowsPrintService>();
        }
        services.AddSingleton<IPdfMerger, PdfMerger>();
        services.AddSingleton<IFileConverterFactory, FileConverterFactory>();
        services.AddSingleton<ICoverPageGenerator, CoverPageGenerator>();
        services.AddSingleton<IRetryPolicy, DefaultRetryPolicy>();
        services.AddSingleton<IJobStageRetryDecider, DefaultRetryPolicy>();
        services.AddSingleton<IUIService, UIService>();

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
        if (licenseKeys != null && licenseKeys.Length > 0)
        {
            foreach (var key in licenseKeys)
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(key);
                }
            }
        }
    }

    internal static IReadOnlyList<string> NormalizeSerilogFilePaths(IConfiguration configuration)
    {
        var writeToSection = configuration.GetSection("Serilog:WriteTo");
        var filePaths = new List<string>();
        var customLogDirectory = configuration["ApplicationSettings:Logging:Path"];

        foreach (var sink in writeToSection.GetChildren())
        {
            var sinkName = sink["Name"];
            if (!string.Equals(sinkName, "File", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var rawPath = sink.GetSection("Args")["path"];
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                continue;
            }

            try
            {
                var expandedPath = Environment.ExpandEnvironmentVariables(rawPath);

                if (!string.IsNullOrWhiteSpace(customLogDirectory))
                {
                    var fileName = Path.GetFileName(expandedPath);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        expandedPath = Path.Combine(customLogDirectory, fileName);
                    }
                }

                if (!string.Equals(rawPath, expandedPath, StringComparison.Ordinal))
                {
                    configuration[$"{sink.Path}:Args:path"] = expandedPath;
                }

                filePaths.Add(expandedPath);
            }
            catch
            {
                // 如果目录创建失败，保持继续，Serilog 在初始化时会记录相应错误。
            }
        }

        return filePaths;
    }

    internal static void EnsureSerilogDirectories(IEnumerable<string> filePaths)
    {
        foreach (var path in filePaths)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch
            {
                // 如果目录创建失败，保持继续，Serilog 在初始化时会记录相应错误。
            }
        }
    }
}

