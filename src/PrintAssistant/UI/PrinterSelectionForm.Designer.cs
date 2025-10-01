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
            cmbPrinters = new ComboBox();
            lblCopies = new Label();
            numCopies = new NumericUpDown();
            btnOK = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)numCopies).BeginInit();
            SuspendLayout();
            // 
            // lblPrinter
            // 
            lblPrinter.AutoSize = true;
            lblPrinter.Location = new Point(28, 27);
            lblPrinter.Name = "lblPrinter";
            lblPrinter.Size = new Size(80, 17);
            lblPrinter.TabIndex = 0;
            lblPrinter.Text = "选择打印机:";
            // 
            // cmbPrinters
            // 
            cmbPrinters.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPrinters.FormattingEnabled = true;
            cmbPrinters.Location = new Point(114, 24);
            cmbPrinters.Name = "cmbPrinters";
            cmbPrinters.Size = new Size(288, 25);
            cmbPrinters.TabIndex = 1;
            // 
            // lblCopies
            // 
            lblCopies.AutoSize = true;
            lblCopies.Location = new Point(40, 70);
            lblCopies.Name = "lblCopies";
            lblCopies.Size = new Size(68, 17);
            lblCopies.TabIndex = 2;
            lblCopies.Text = "打印份数:";
            // 
            // numCopies
            // 
            numCopies.Location = new Point(114, 68);
            numCopies.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCopies.Name = "numCopies";
            numCopies.Size = new Size(120, 23);
            numCopies.TabIndex = 3;
            numCopies.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnOK
            // 
            btnOK.Location = new Point(226, 115);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(85, 30);
            btnOK.TabIndex = 4;
            btnOK.Text = "确定";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(317, 115);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(85, 30);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // PrinterSelectionForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(434, 161);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(numCopies);
            Controls.Add(lblCopies);
            Controls.Add(cmbPrinters);
            Controls.Add(lblPrinter);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PrinterSelectionForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "选择打印机";
            Load += PrinterSelectionForm_Load;
            ((System.ComponentModel.ISupportInitialize)numCopies).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblPrinter;
        private ComboBox cmbPrinters;
        private Label lblCopies;
        private NumericUpDown numCopies;
        private Button btnOK;
        private Button btnCancel;
    }
}

