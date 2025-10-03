using Microsoft.Extensions.Options;
using PrintAssistant.Configuration;
using System;
using System.Drawing.Printing;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace PrintAssistant.UI;

/// <summary>
/// 应用程序的配置窗口。
/// </summary>
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

    private void SettingsForm_Load(object sender, EventArgs e)
    {
        LoadSettings();
    }

    /// <summary>
    /// 从配置对象加载设置并填充到UI控件。
    /// </summary>
    private void LoadSettings()
    {
        txtMonitorPath.Text = _settings.Monitoring.Path;
        numDebounceInterval.Value = _settings.Monitoring.DebounceIntervalMilliseconds;
        numMaxFileSize.Value = _settings.Monitoring.MaxFileSizeMegaBytes;

        chkGenerateCoverPage.Checked = _settings.Printing.GenerateCoverPage;
        LoadPrintersForExclusion();

        txtLogPath.Text = _settings.Logging.Path;
    }

    /// <summary>
    /// 加载打印机列表到复选框，并根据配置勾选需要排除的打印机。
    /// </summary>
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
        using var dialog = new FolderBrowserDialog
        {
            Description = "请选择要监控的文件夹"
        };
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtMonitorPath.Text = dialog.SelectedPath;
        }
    }

    private void btnBrowseLogPath_Click(object sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "请选择日志文件存储位置"
        };
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

    /// <summary>
    /// 从UI控件收集设置，更新配置对象，并将其序列化写回 appsettings.json 文件。
    /// </summary>
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
                if (property.NameEquals("ApplicationSettings"))
                {
                    writer.WritePropertyName("ApplicationSettings");
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

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }
}

