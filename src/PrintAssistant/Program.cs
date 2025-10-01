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
        ApplicationConfiguration.Initialize();

        var builder = Host.CreateApplicationBuilder(args);

        // 尝试加载 appsettings.Secret.json，以便从中读取 Syncfusion 许可证
        builder.Configuration.AddJsonFile("appsettings.Secret.json", optional: true, reloadOnChange: true);
        RegisterSyncfusionLicense(builder.Configuration);

        // 配置 Serilog
        builder.Services.AddSerilog((services, loggerConfiguration) =>
            loggerConfiguration.ReadFrom.Configuration(builder.Configuration));

        ConfigureServices(builder.Services, builder.Configuration);

        using var host = builder.Build();

        // 确保托盘图标服务被实例化以注册通知图标
        _ = host.Services.GetRequiredService<ITrayIconService>();

        host.Start();

        Application.Run();

        host.StopAsync().GetAwaiter().GetResult();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // 配置强类型选项
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        // 文件系统抽象
        services.AddSingleton<IFileSystem, FileSystem>();

        // 核心服务注册
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
}

