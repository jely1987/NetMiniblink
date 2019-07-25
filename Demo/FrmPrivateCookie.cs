using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QQ2564874169.Miniblink;

namespace Demo
{
    public partial class FrmPrivateCookie : Form
    {
        public FrmPrivateCookie()
        {
            InitializeComponent();
        }

        private void FrmPrivateCookie_Load(object sender, EventArgs e)
        {
            textBox1.Text = "https://gitee.com";
            button1_Click(null, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var url = textBox1.Text;
            if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase) == false)
            {
                url = "http://" + url;
            }
            var tab = new TabPage();
            var bro = new MiniblinkBrowser();
            tab.Controls.Add(bro);
            tabs.TabPages.Add(tab);
            bro.Dock = DockStyle.Fill;
            bro.DocumentReady += (rs, re) =>
            {
                var t = bro.DocumentTitle;
                if (t.Length > 20)
                    t = t.Substring(0, 20) + "...";
                tab.Text = t;
            };
//            bro.SetProxy(new WKEProxy
//            {
//                HostName = "127.0.0.1",
//                Port = 8888,
//                Type = wkeProxyType.HTTP,
//            });
            bro.LoadUri(url);
        }
    }
}
