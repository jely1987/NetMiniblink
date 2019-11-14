using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QQ2564874169.Miniblink;
using QQ2564874169.Miniblink.LoadResourceImpl;

namespace Demo
{
    public partial class FrmTab : Form
    {
        private string dir = Path.Combine(Application.StartupPath, "webres");

        public FrmTab()
        {
            InitializeComponent();
        }

        private void FrmTab_Load(object sender, EventArgs e)
        {
            NewPage("http://loc.web/nav.html");
        }

        [NetFunc]
        private void NewPage(string url)
        {
            var tab = new TabPage((tabControl1.TabCount + 1) + " page");
            var bw = new MiniblinkBrowser
            {
                Dock = DockStyle.Fill
            };
            bw.RegisterNetFunc(this);
            bw.LoadResourceHandlerList.Add(new LoadResourceByFile(dir, "loc.web"));
            tab.Controls.Add(bw);
            tabControl1.TabPages.Add(tab);
            tabControl1.SelectedTab = tab;
            bw.LoadUri(url);
        }
    }
}
