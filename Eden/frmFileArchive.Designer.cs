namespace Eden
{
    partial class frmFileArchive
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFileArchive));
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            button1 = new Button();
            textBox1 = new TextBox();
            label1 = new Label();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            tabPage2 = new TabPage();
            button2 = new Button();
            listView2 = new ListView();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            comboBox1 = new ComboBox();
            label2 = new Label();
            statusStrip2 = new StatusStrip();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            splitContainer1 = new SplitContainer();
            richTextBox1 = new RichTextBox();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            statusStrip1.SuspendLayout();
            tabPage2.SuspendLayout();
            statusStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Margin = new Padding(4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(492, 337);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(button1);
            tabPage1.Controls.Add(textBox1);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(listView1);
            tabPage1.Controls.Add(statusStrip1);
            tabPage1.Location = new Point(4, 28);
            tabPage1.Margin = new Padding(4);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(4);
            tabPage1.Size = new Size(484, 305);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Compress";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new Point(339, 7);
            button1.Name = "button1";
            button1.Size = new Size(137, 27);
            button1.TabIndex = 4;
            button1.Text = "Go";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(128, 7);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(205, 27);
            textBox1.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(8, 10);
            label1.Name = "label1";
            label1.Size = new Size(114, 19);
            label1.TabIndex = 2;
            label1.Text = "Archive Name :";
            // 
            // listView1
            // 
            listView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1 });
            listView1.FullRowSelect = true;
            listView1.Location = new Point(4, 40);
            listView1.Name = "listView1";
            listView1.Size = new Size(476, 298);
            listView1.TabIndex = 1;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "FileName";
            columnHeader1.Width = 400;
            // 
            // statusStrip1
            // 
            statusStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(4, 277);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(476, 24);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(158, 19);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(button2);
            tabPage2.Controls.Add(listView2);
            tabPage2.Controls.Add(comboBox1);
            tabPage2.Controls.Add(label2);
            tabPage2.Controls.Add(statusStrip2);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Margin = new Padding(4);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(4);
            tabPage2.Size = new Size(484, 309);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Extract";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(347, 7);
            button2.Name = "button2";
            button2.Size = new Size(129, 27);
            button2.TabIndex = 4;
            button2.Text = "Go";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // listView2
            // 
            listView2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listView2.Columns.AddRange(new ColumnHeader[] { columnHeader3, columnHeader4 });
            listView2.FullRowSelect = true;
            listView2.Location = new Point(9, 40);
            listView2.Name = "listView2";
            listView2.Size = new Size(468, 238);
            listView2.TabIndex = 3;
            listView2.UseCompatibleStateImageBehavior = false;
            listView2.View = View.Details;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "FileName";
            columnHeader3.Width = 200;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Status";
            columnHeader4.Width = 200;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(86, 7);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(255, 27);
            comboBox1.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(9, 10);
            label2.Name = "label2";
            label2.Size = new Size(71, 19);
            label2.TabIndex = 1;
            label2.Text = "Method :";
            // 
            // statusStrip2
            // 
            statusStrip2.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
            statusStrip2.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel2 });
            statusStrip2.Location = new Point(4, 281);
            statusStrip2.Name = "statusStrip2";
            statusStrip2.Size = new Size(476, 24);
            statusStrip2.TabIndex = 0;
            statusStrip2.Text = "statusStrip2";
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new Size(158, 19);
            toolStripStatusLabel2.Text = "toolStripStatusLabel2";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel2;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(richTextBox1);
            splitContainer1.Size = new Size(492, 540);
            splitContainer1.SplitterDistance = 337;
            splitContainer1.TabIndex = 1;
            // 
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new Point(0, 0);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(492, 199);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // frmFileArchive
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(492, 540);
            Controls.Add(splitContainer1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            Name = "frmFileArchive";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmFileArchive";
            FormClosed += frmFileArchive_FormClosed;
            Load += frmFileArchive_Load;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            statusStrip2.ResumeLayout(false);
            statusStrip2.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private StatusStrip statusStrip1;
        private TabPage tabPage2;
        private StatusStrip statusStrip2;
        private ListView listView1;
        private ColumnHeader columnHeader1;
        private Button button1;
        private TextBox textBox1;
        private Label label1;
        private Button button2;
        private ListView listView2;
        private ComboBox comboBox1;
        private Label label2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private SplitContainer splitContainer1;
        private RichTextBox richTextBox1;
    }
}