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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblPrinter = new Label();
            printerFlowPanel = new FlowLayoutPanel();
            lblCopies = new Label();
            numCopies = new NumericUpDown();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)numCopies).BeginInit();
            SuspendLayout();
            // 
            // lblPrinter
            // 
            lblPrinter.AutoSize = true;
            lblPrinter.Location = new Point(20, 18);
            lblPrinter.Name = "lblPrinter";
            lblPrinter.Size = new Size(80, 17);
            lblPrinter.TabIndex = 0;
            lblPrinter.Text = "选择打印机:";
            // 
            // printerFlowPanel
            // 
            printerFlowPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            printerFlowPanel.AutoScroll = true;
            printerFlowPanel.BorderStyle = BorderStyle.FixedSingle;
            printerFlowPanel.Location = new Point(23, 43);
            printerFlowPanel.Name = "printerFlowPanel";
            printerFlowPanel.Size = new Size(488, 180);
            printerFlowPanel.TabIndex = 1;
            // 
            // lblCopies
            // 
            lblCopies.AutoSize = true;
            lblCopies.Location = new Point(23, 240);
            lblCopies.Name = "lblCopies";
            lblCopies.Size = new Size(68, 17);
            lblCopies.TabIndex = 2;
            lblCopies.Text = "打印份数:";
            // 
            // numCopies
            // 
            numCopies.Location = new Point(97, 238);
            numCopies.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCopies.Name = "numCopies";
            numCopies.Size = new Size(120, 23);
            numCopies.TabIndex = 3;
            numCopies.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(426, 287);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(85, 30);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // PrinterSelectionForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(534, 329);
            Controls.Add(btnCancel);
            Controls.Add(numCopies);
            Controls.Add(lblCopies);
            Controls.Add(printerFlowPanel);
            Controls.Add(lblPrinter);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PrinterSelectionForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "选择打印机";
            TopMost = true;
            Load += PrinterSelectionForm_Load;
            ((System.ComponentModel.ISupportInitialize)numCopies).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblPrinter;
        private FlowLayoutPanel printerFlowPanel;
        private Label lblCopies;
        private NumericUpDown numCopies;
        private Button btnCancel;
    }
}

