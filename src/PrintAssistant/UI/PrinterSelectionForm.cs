using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace PrintAssistant.UI;

/// <summary>
/// 一个模态对话框，用于让用户选择打印机和打印份数。
/// </summary>
public partial class PrinterSelectionForm : Form
{
    /// <summary>
    /// 获取用户选择的打印机名称。
    /// </summary>
    public string? SelectedPrinter { get; private set; }

    /// <summary>
    /// 获取用户选择的打印份数。
    /// </summary>
    public int PrintCopies { get; private set; }

    private readonly List<string> _excludedPrinters;

    /// <summary>
    /// 初始化 PrinterSelectionForm 的新实例。
    /// </summary>
    /// <param name="excludedPrinters">不应向用户显示的打印机名称列表。</param>
    public PrinterSelectionForm(List<string> excludedPrinters)
    {
        InitializeComponent();
        _excludedPrinters = excludedPrinters;
        TopMost = true;
    }

    private void PrinterSelectionForm_Load(object sender, EventArgs e)
    {
        LoadPrinters();
    }

    /// <summary>
    /// 加载系统已安装的打印机列表，并过滤掉被排除的打印机。
    /// </summary>
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

