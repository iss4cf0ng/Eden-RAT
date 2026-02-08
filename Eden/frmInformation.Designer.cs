namespace Eden
{
    partial class frmInformation
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            richTextBox1 = new RichTextBox();
            tabPage2 = new TabPage();
            tabPage3 = new TabPage();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            statusStrip2 = new StatusStrip();
            statusStrip3 = new StatusStrip();
            listView1 = new ListView();
            listView2 = new ListView();
            tabPage4 = new TabPage();
            toolStrip1 = new ToolStrip();
            statusStrip4 = new StatusStrip();
            listView3 = new ListView();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            statusStrip1.SuspendLayout();
            tabPage4.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Margin = new Padding(4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(915, 557);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(richTextBox1);
            tabPage1.Controls.Add(statusStrip1);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Margin = new Padding(4);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(4);
            tabPage1.Size = new Size(907, 525);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Basic";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(4, 4);
            richTextBox1.Margin = new Padding(4);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(899, 495);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(listView1);
            tabPage2.Controls.Add(statusStrip2);
            tabPage2.Location = new Point(4, 28);
            tabPage2.Margin = new Padding(4);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(4);
            tabPage2.Size = new Size(907, 525);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "User";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(listView2);
            tabPage3.Controls.Add(statusStrip3);
            tabPage3.Location = new Point(4, 28);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(907, 525);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Application";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(4, 499);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(899, 22);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(128, 17);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // statusStrip2
            // 
            statusStrip2.Location = new Point(4, 499);
            statusStrip2.Name = "statusStrip2";
            statusStrip2.Size = new Size(899, 22);
            statusStrip2.TabIndex = 0;
            statusStrip2.Text = "statusStrip2";
            // 
            // statusStrip3
            // 
            statusStrip3.Location = new Point(0, 503);
            statusStrip3.Name = "statusStrip3";
            statusStrip3.Size = new Size(907, 22);
            statusStrip3.TabIndex = 0;
            statusStrip3.Text = "statusStrip3";
            // 
            // listView1
            // 
            listView1.Dock = DockStyle.Fill;
            listView1.Location = new Point(4, 4);
            listView1.Name = "listView1";
            listView1.Size = new Size(899, 495);
            listView1.TabIndex = 1;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // listView2
            // 
            listView2.Dock = DockStyle.Fill;
            listView2.Location = new Point(0, 0);
            listView2.Name = "listView2";
            listView2.Size = new Size(907, 503);
            listView2.TabIndex = 1;
            listView2.UseCompatibleStateImageBehavior = false;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(listView3);
            tabPage4.Controls.Add(statusStrip4);
            tabPage4.Controls.Add(toolStrip1);
            tabPage4.Location = new Point(4, 28);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(907, 525);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Bash History";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(907, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip4
            // 
            statusStrip4.Location = new Point(0, 503);
            statusStrip4.Name = "statusStrip4";
            statusStrip4.Size = new Size(907, 22);
            statusStrip4.TabIndex = 1;
            statusStrip4.Text = "statusStrip4";
            // 
            // listView3
            // 
            listView3.Dock = DockStyle.Fill;
            listView3.Location = new Point(0, 25);
            listView3.Name = "listView3";
            listView3.Size = new Size(907, 478);
            listView3.TabIndex = 2;
            listView3.UseCompatibleStateImageBehavior = false;
            // 
            // frmInformation
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(915, 557);
            Controls.Add(tabControl1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
            Margin = new Padding(4);
            Name = "frmInformation";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmInformation";
            FormClosed += frmInformation_FormClosed;
            Load += frmInformation_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private RichTextBox richTextBox1;
        private TabPage tabPage2;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ListView listView1;
        private StatusStrip statusStrip2;
        private TabPage tabPage3;
        private ListView listView2;
        private StatusStrip statusStrip3;
        private TabPage tabPage4;
        private ListView listView3;
        private StatusStrip statusStrip4;
        private ToolStrip toolStrip1;
    }
}