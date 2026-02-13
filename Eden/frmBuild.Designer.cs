namespace Eden
{
    partial class frmBuild
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
            label1 = new Label();
            textBox1 = new TextBox();
            numericUpDown1 = new NumericUpDown();
            comboBox1 = new ComboBox();
            comboBox2 = new ComboBox();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            groupBox1 = new GroupBox();
            button1 = new Button();
            button2 = new Button();
            label5 = new Label();
            comboBox3 = new ComboBox();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            numericUpDown3 = new NumericUpDown();
            label7 = new Label();
            label6 = new Label();
            numericUpDown2 = new NumericUpDown();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            label8 = new Label();
            label9 = new Label();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(39, 60);
            label1.Name = "label1";
            label1.Size = new Size(46, 19);
            label1.TabIndex = 0;
            label1.Text = "IPv4 :";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(91, 57);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(228, 27);
            textBox1.TabIndex = 1;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(91, 90);
            numericUpDown1.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(120, 27);
            numericUpDown1.TabIndex = 2;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(91, 26);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(228, 27);
            comboBox1.TabIndex = 3;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(91, 59);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(232, 27);
            comboBox2.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(14, 29);
            label2.Name = "label2";
            label2.Size = new Size(71, 19);
            label2.TabIndex = 5;
            label2.Text = "Listener :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(40, 92);
            label3.Name = "label3";
            label3.Size = new Size(45, 19);
            label3.TabIndex = 6;
            label3.Text = "Port :";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(27, 62);
            label4.Name = "label4";
            label4.Size = new Size(58, 19);
            label4.TabIndex = 8;
            label4.Text = "Obfus :";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button1);
            groupBox1.Controls.Add(comboBox1);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(numericUpDown1);
            groupBox1.Controls.Add(label3);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(337, 123);
            groupBox1.TabIndex = 9;
            groupBox1.TabStop = false;
            groupBox1.Text = "Host";
            // 
            // button1
            // 
            button1.Location = new Point(213, 88);
            button1.Name = "button1";
            button1.Size = new Size(112, 29);
            button1.TabIndex = 10;
            button1.Text = "Test";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(12, 351);
            button2.Name = "button2";
            button2.Size = new Size(337, 49);
            button2.TabIndex = 11;
            button2.Text = "Build";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(43, 29);
            label5.Name = "label5";
            label5.Size = new Size(42, 19);
            label5.TabIndex = 13;
            label5.Text = "Tag :";
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Location = new Point(91, 26);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(232, 27);
            comboBox3.TabIndex = 12;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(comboBox3);
            groupBox2.Controls.Add(comboBox2);
            groupBox2.Controls.Add(label4);
            groupBox2.Location = new Point(12, 141);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(337, 103);
            groupBox2.TabIndex = 14;
            groupBox2.TabStop = false;
            groupBox2.Text = "Client";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(label9);
            groupBox3.Controls.Add(label8);
            groupBox3.Controls.Add(numericUpDown3);
            groupBox3.Controls.Add(label7);
            groupBox3.Controls.Add(label6);
            groupBox3.Controls.Add(numericUpDown2);
            groupBox3.Location = new Point(12, 250);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(337, 95);
            groupBox3.TabIndex = 15;
            groupBox3.TabStop = false;
            groupBox3.Text = "Connection";
            // 
            // numericUpDown3
            // 
            numericUpDown3.Location = new Point(103, 59);
            numericUpDown3.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numericUpDown3.Name = "numericUpDown3";
            numericUpDown3.Size = new Size(156, 27);
            numericUpDown3.TabIndex = 3;
            numericUpDown3.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(14, 62);
            label7.Name = "label7";
            label7.Size = new Size(83, 19);
            label7.TabIndex = 2;
            label7.Text = "Send Info :";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(44, 29);
            label6.Name = "label6";
            label6.Size = new Size(53, 19);
            label6.TabIndex = 1;
            label6.Text = "Retry :";
            // 
            // numericUpDown2
            // 
            numericUpDown2.Location = new Point(103, 26);
            numericUpDown2.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new Size(156, 27);
            numericUpDown2.TabIndex = 0;
            numericUpDown2.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // statusStrip1
            // 
            statusStrip1.Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 409);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(358, 24);
            statusStrip1.TabIndex = 16;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(158, 19);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(265, 29);
            label8.Name = "label8";
            label8.Size = new Size(31, 19);
            label8.TabIndex = 4;
            label8.Text = "sec";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(265, 62);
            label9.Name = "label9";
            label9.Size = new Size(31, 19);
            label9.TabIndex = 5;
            label9.Text = "sec";
            // 
            // frmBuild
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(358, 433);
            Controls.Add(statusStrip1);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(button2);
            Controls.Add(groupBox1);
            Font = new Font("Microsoft JhengHei UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 136);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            MaximizeBox = false;
            Name = "frmBuild";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "frmBuild";
            FormClosed += frmBuild_FormClosed;
            Load += frmBuild_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private NumericUpDown numericUpDown1;
        private ComboBox comboBox1;
        private ComboBox comboBox2;
        private Label label2;
        private Label label3;
        private Label label4;
        private GroupBox groupBox1;
        private Button button1;
        private Button button2;
        private Label label5;
        private ComboBox comboBox3;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private NumericUpDown numericUpDown2;
        private Label label7;
        private Label label6;
        private NumericUpDown numericUpDown3;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private Label label9;
        private Label label8;
    }
}