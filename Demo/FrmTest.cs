using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QQ2564874169.Miniblink;
using QQ2564874169.Miniblink.ResourceLoader;

namespace Demo
{
    public partial class FrmTest : MiniblinkForm
    {
        public FrmTest()
        {
            InitializeComponent();
            ResourceLoader.Add(new EmbedLoader(typeof(FrmMain).Assembly, "Res", "loc.res"));
        }

        private void FrmTest_Load(object sender, EventArgs e)
        {
            LoadUrlBegin += FrmTest_LoadUrlBegin;
            NetResponse += FrmTest_NetResponse;
            LoadUri("http://www.baidu.com");
        }

        private void FrmTest_NetResponse(object sender, NetResponseEventArgs e)
        {
            Console.WriteLine("resp = " + e.RequestMethod);
        }

        private void FrmTest_LoadUrlBegin(object sender, LoadUrlBeginEventArgs e)
        {
            e.Response(a => { Console.WriteLine($"{a.RequestMethod} = {e.Url}"); });
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Reload();
        }

        [NetFunc]
        private void cunzai()
        {
            MessageBox.Show("66666");
        }

        [NetFunc(BindToSubFrame = false)]
        private void bucunzai()
        {

        }
    }
}
