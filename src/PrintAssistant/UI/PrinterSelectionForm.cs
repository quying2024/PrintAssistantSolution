using System;
using System.Collections.Generic;
using System.Drawing;
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

    private List<string> _excludedPrinters = new();
    private List<string> _availablePrinters = new();

    public PrinterSelectionForm()
    {
        InitializeComponent();
    }

    public void Initialize(IEnumerable<string> excludedPrinters, string? currentPrinter, int currentCopies)
    {
        _excludedPrinters = excludedPrinters?.ToList() ?? new List<string>();
        _availablePrinters = LoadAvailablePrinters();

        BuildPrinterButtons(currentPrinter);
        numCopies.Value = Math.Max(1, currentCopies);
    }

    private void PrinterSelectionForm_Load(object sender, EventArgs e)
    {
        if (_availablePrinters.Count == 0)
        {
            _availablePrinters = LoadAvailablePrinters();
            BuildPrinterButtons(null);
        }
    }

    private List<string> LoadAvailablePrinters()
    {
        return PrinterSettings.InstalledPrinters.Cast<string>()
            .Where(p => !_excludedPrinters.Contains(p, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }

    private void BuildPrinterButtons(string? preferredPrinter)
    {
        printerFlowPanel.Controls.Clear();

        if (_availablePrinters.Count == 0)
        {
            var label = new Label
            {
                Text = "未检测到可用打印机",
                AutoSize = true,
                ForeColor = Color.DarkRed,
                Padding = new Padding(5)
            };
            printerFlowPanel.Controls.Add(label);
            return;
        }

        string defaultPrinter = new PrinterSettings().PrinterName;
        string? activePrinter = !string.IsNullOrWhiteSpace(preferredPrinter) && _availablePrinters.Contains(preferredPrinter)
            ? preferredPrinter
            : (_availablePrinters.Contains(defaultPrinter) ? defaultPrinter : _availablePrinters.First());

        foreach (var printer in _availablePrinters)
        {
            var button = new Button
            {
                Text = printer,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(12, 6, 12, 6),
                Margin = new Padding(6),
                Tag = printer,
                BackColor = string.Equals(printer, activePrinter, StringComparison.OrdinalIgnoreCase)
                    ? Color.LightSteelBlue
                    : SystemColors.Control,
                FlatStyle = FlatStyle.Standard
            };

            button.Click += (_, _) => SelectPrinterAndClose(printer);
            printerFlowPanel.Controls.Add(button);
        }
    }

    private void SelectPrinterAndClose(string printer)
    {
        SelectedPrinter = printer;
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

