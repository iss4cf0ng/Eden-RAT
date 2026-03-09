using ICSharpCode.TextEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eden
{
    public partial class frmShowPayload : Form
    {
        private TextEditorControl editor = new TextEditorControl();
        private string m_szPayload { get; set; }

        public frmShowPayload()
        {
            InitializeComponent();

            Text = "Payload";
            m_szPayload = string.Empty;
        }

        public void fnShowPayload(string szPayload)
        {
            m_szPayload = szPayload;
            editor.Text = szPayload;
            editor.Refresh();

            toolStripComboBox1.SelectedIndex = 0;
            toolStripStatusLabel1.Text = $"Length[{editor.Text.Length}]";
        }

        void fnSetup()
        {
            Controls.Add(editor);

            editor.BringToFront();
            editor.Dock = DockStyle.Fill;
            editor.Text = string.Empty;
            editor.IsReadOnly = false;
            editor.ActiveTextAreaControl.HScrollBar.Visible = true;
            editor.TextEditorProperties.EnableFolding = false;
            editor.AutoScroll = true;
            editor.Refresh();

            toolStripComboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            toolStripComboBox1.Items.Add("Raw");
            toolStripComboBox1.Items.Add("Command");
        }

        private void frmShowPayload_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Python file(*.py)|*.py";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(sfd.FileName, editor.Text);
                    MessageBox.Show("Save payload successfully: " + sfd.FileName, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(editor.Text);
            toolStripStatusLabel1.Text = "Copy successfully.";
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (toolStripComboBox1.SelectedIndex)
            {
                case 0:
                    editor.Text = m_szPayload;
                    editor.Refresh();
                    break;
                case 1:
                    editor.Text = $"python3 -c \"{m_szPayload}\"";
                    editor.Refresh();
                    break;
            }
        }
    }
}
