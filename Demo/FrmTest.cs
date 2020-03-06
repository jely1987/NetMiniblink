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
            View.ResourceLoader.Add(new EmbedLoader(typeof(FrmMain).Assembly, "Res", "loc.res"));
        }

        private void FrmTest_Load(object sender, EventArgs e)
        {
            View.RequestBefore += View_RequestBefore;
            View.LoadUri("https://www.baidu.com");
        }

        private void View_RequestBefore(object sender, RequestEventArgs e)
        {
            e.Response += e_response;
        }

        private void e_response(object sender, ResponseEventArgs e)
        {
            Console.WriteLine(e.Url);
            var head = e.GetHeaders();
            foreach (var name in head.Keys)
            {
                Console.WriteLine($"\t{name} = {head[name]}");
            }
        }

        private void url_net_data(object sender, NetDataEventArgs e)
        {
            Console.WriteLine(e.Url);
            var head = e.GetHeaders();
            foreach (var name in head.Keys)
            {
                Console.WriteLine($"\t{name} = {head[name]}");
            }
        }

        private void url_load_fail(object sender, EventArgs e)
        {
            var req = (RequestEventArgs) sender;
            Console.WriteLine($"{req.Method} = {req.Url}");
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            View.ShowDevTools();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            View.Reload();
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
