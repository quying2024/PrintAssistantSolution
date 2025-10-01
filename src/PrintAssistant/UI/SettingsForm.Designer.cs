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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            numMaxFileSize = new NumericUpDown();
            label3 = new Label();
            numDebounceInterval = new NumericUpDown();
            label2 = new Label();
            btnBrowseMonitorPath = new Button();
            txtMonitorPath = new TextBox();
            label1 = new Label();
            groupBox2 = new GroupBox();
            clbExcludedPrinters = new CheckedListBox();
            label4 = new Label();
            chkGenerateCoverPage = new CheckBox();
            groupBox3 = new GroupBox();
            btnBrowseLogPath = new Button();
            txtLogPath = new TextBox();
            label5 = new Label();
            btnSave = new Button();
            btnCancel = new Button();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMaxFileSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDebounceInterval).BeginInit();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(numMaxFileSize);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(numDebounceInterval);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(btnBrowseMonitorPath);
            groupBox1.Controls.Add(txtMonitorPath);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(560, 150);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "监控设置";
            // 
            // numMaxFileSize
            // 
            numMaxFileSize.Location = new Point(150, 105);
            numMaxFileSize.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numMaxFileSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numMaxFileSize.Name = "numMaxFileSize";
            numMaxFileSize.Size = new Size(120, 23);
            numMaxFileSize.TabIndex = 6;
            numMaxFileSize.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(18, 107);
            label3.Name = "label3";
            label3.Size = new Size(126, 17);
            label3.TabIndex = 5;
            label3.Text = "最大文件大小 (MB):";
            // 
            // numDebounceInterval
            // 
            numDebounceInterval.Location = new Point(150, 68);
            numDebounceInterval.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numDebounceInterval.Minimum = new decimal(new int[] { 500, 0, 0, 0 });
            numDebounceInterval.Name = "numDebounceInterval";
            numDebounceInterval.Size = new Size(120, 23);
            numDebounceInterval.TabIndex = 4;
            numDebounceInterval.Value = new decimal(new int[] { 2500, 0, 0, 0 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(18, 70);
            label2.Name = "label2";
            label2.Size = new Size(115, 17);
            label2.TabIndex = 3;
            label2.Text = "任务聚合延时(ms):";
            // 
            // btnBrowseMonitorPath
            // 
            btnBrowseMonitorPath.Location = new Point(470, 29);
            btnBrowseMonitorPath.Name = "btnBrowseMonitorPath";
            btnBrowseMonitorPath.Size = new Size(75, 25);
            btnBrowseMonitorPath.TabIndex = 2;
            btnBrowseMonitorPath.Text = "浏览...";
            btnBrowseMonitorPath.UseVisualStyleBackColor = true;
            btnBrowseMonitorPath.Click += btnBrowseMonitorPath_Click;
            // 
            // txtMonitorPath
            // 
            txtMonitorPath.Location = new Point(150, 30);
            txtMonitorPath.Name = "txtMonitorPath";
            txtMonitorPath.Size = new Size(314, 23);
            txtMonitorPath.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(18, 33);
            label1.Name = "label1";
            label1.Size = new Size(80, 17);
            label1.TabIndex = 0;
            label1.Text = "监控文件夹:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(clbExcludedPrinters);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(chkGenerateCoverPage);
            groupBox2.Location = new Point(12, 168);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(560, 200);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "打印设置";
            // 
            // clbExcludedPrinters
            // 
            clbExcludedPrinters.FormattingEnabled = true;
            clbExcludedPrinters.Location = new Point(150, 60);
            clbExcludedPrinters.Name = "clbExcludedPrinters";
            clbExcludedPrinters.Size = new Size(395, 130);
            clbExcludedPrinters.TabIndex = 2;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(18, 60);
            label4.Name = "label4";
            label4.Size = new Size(104, 17);
            label4.TabIndex = 1;
            label4.Text = "不显示的打印机:";
            // 
            // chkGenerateCoverPage
            // 
            chkGenerateCoverPage.AutoSize = true;
            chkGenerateCoverPage.Location = new Point(21, 28);
            chkGenerateCoverPage.Name = "chkGenerateCoverPage";
            chkGenerateCoverPage.Size = new Size(123, 21);
            chkGenerateCoverPage.TabIndex = 0;
            chkGenerateCoverPage.Text = "打印任务封面页";
            chkGenerateCoverPage.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(btnBrowseLogPath);
            groupBox3.Controls.Add(txtLogPath);
            groupBox3.Controls.Add(label5);
            groupBox3.Location = new Point(12, 374);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(560, 75);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "日志设置";
            // 
            // btnBrowseLogPath
            // 
            btnBrowseLogPath.Location = new Point(470, 29);
            btnBrowseLogPath.Name = "btnBrowseLogPath";
            btnBrowseLogPath.Size = new Size(75, 25);
            btnBrowseLogPath.TabIndex = 5;
            btnBrowseLogPath.Text = "浏览...";
            btnBrowseLogPath.UseVisualStyleBackColor = true;
            btnBrowseLogPath.Click += btnBrowseLogPath_Click;
            // 
            // txtLogPath
            // 
            txtLogPath.Location = new Point(150, 30);
            txtLogPath.Name = "txtLogPath";
            txtLogPath.Size = new Size(314, 23);
            txtLogPath.TabIndex = 4;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(18, 33);
            label5.Name = "label5";
            label5.Size = new Size(92, 17);
            label5.TabIndex = 3;
            label5.Text = "日志文件目录:";
            // 
            // btnSave
            // 
            btnSave.Location = new Point(396, 465);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(85, 30);
            btnSave.TabIndex = 3;
            btnSave.Text = "保存";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(487, 465);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(85, 30);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // SettingsForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(584, 511);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "设置";
            Load += SettingsForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMaxFileSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDebounceInterval).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private NumericUpDown numMaxFileSize;
        private Label label3;
        private NumericUpDown numDebounceInterval;
        private Label label2;
        private Button btnBrowseMonitorPath;
        private TextBox txtMonitorPath;
        private Label label1;
        private GroupBox groupBox2;
        private CheckedListBox clbExcludedPrinters;
        private Label label4;
        private CheckBox chkGenerateCoverPage;
        private GroupBox groupBox3;
        private Button btnBrowseLogPath;
        private TextBox txtLogPath;
        private Label label5;
        private Button btnSave;
        private Button btnCancel;
    }
}

