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
            checkBox1 = new CheckBox();
            label4 = new Label();
            groupBox1 = new GroupBox();
            button1 = new Button();
            button2 = new Button();
            label5 = new Label();
            comboBox3 = new ComboBox();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            numericUpDown2 = new NumericUpDown();
            label6 = new Label();
            label7 = new Label();
            numericUpDown3 = new NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(25, 32);
            label1.Name = "label1";
            label1.Size = new Size(29, 19);
            label1.TabIndex = 0;
            label1.Text = "IP :";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(60, 26);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(197, 27);
            textBox1.TabIndex = 1;
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new Point(60, 59);
            numericUpDown1.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new Size(120, 27);
            numericUpDown1.TabIndex = 2;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(107, 59);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(126, 27);
            comboBox1.TabIndex = 3;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(107, 92);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(126, 27);
            comboBox2.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(30, 62);
            label2.Name = "label2";
            label2.Size = new Size(71, 19);
            label2.TabIndex = 5;
            label2.Text = "Listener :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(9, 61);
            label3.Name = "label3";
            label3.Size = new Size(45, 19);
            label3.TabIndex = 6;
            label3.Text = "Port :";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(263, 28);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(60, 23);
            checkBox1.TabIndex = 7;
            checkBox1.Text = "DNS";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(8, 95);
            label4.Name = "label4";
            label4.Size = new Size(93, 19);
            label4.TabIndex = 8;
            label4.Text = "Obfuscator :";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button1);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Controls.Add(checkBox1);
            groupBox1.Controls.Add(numericUpDown1);
            groupBox1.Controls.Add(label3);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(337, 97);
            groupBox1.TabIndex = 9;
            groupBox1.TabStop = false;
            groupBox1.Text = "Host";
            // 
            // button1
            // 
            button1.Location = new Point(189, 57);
            button1.Name = "button1";
            button1.Size = new Size(134, 29);
            button1.TabIndex = 10;
            button1.Text = "Test";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(12, 351);
            button2.Name = "button2";
            button2.Size = new Size(337, 49);
            button2.TabIndex = 11;
            button2.Text = "Build";
            button2.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(29, 29);
            label5.Name = "label5";
            label5.Size = new Size(72, 19);
            label5.TabIndex = 13;
            label5.Text = "Payload :";
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Location = new Point(107, 26);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(126, 27);
            comboBox3.TabIndex = 12;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(comboBox1);
            groupBox2.Controls.Add(comboBox3);
            groupBox2.Controls.Add(comboBox2);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(label4);
            groupBox2.Location = new Point(12, 115);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(337, 129);
            groupBox2.TabIndex = 14;
            groupBox2.TabStop = false;
            groupBox2.Text = "Client";
            // 
            // groupBox3
            // 
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
            // numericUpDown2
            // 
            numericUpDown2.Location = new Point(103, 26);
            numericUpDown2.Name = "numericUpDown2";
            numericUpDown2.Size = new Size(106, 27);
            numericUpDown2.TabIndex = 0;
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
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(14, 62);
            label7.Name = "label7";
            label7.Size = new Size(83, 19);
            label7.TabIndex = 2;
            label7.Text = "Send Info :";
            // 
            // numericUpDown3
            // 
            numericUpDown3.Location = new Point(103, 59);
            numericUpDown3.Name = "numericUpDown3";
            numericUpDown3.Size = new Size(106, 27);
            numericUpDown3.TabIndex = 3;
            // 
            // frmBuild
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(358, 409);
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
            Load += frmBuild_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDown3).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private NumericUpDown numericUpDown1;
        private ComboBox comboBox1;
        private ComboBox comboBox2;
        private Label label2;
        private Label label3;
        private CheckBox checkBox1;
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
    }
}