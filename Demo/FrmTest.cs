using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
            View.ConsoleMessage += View_ConsoleMessage;
            //View.DidCreateScriptContext += View_DidCreateScriptContext;
            View.RequestBefore += View_RequestBefore;
            //View.LoadUri("https://www.baidu.com");
            View.LoadUri("http://loc.res/test.html");
        }

        private void View_RequestBefore(object sender, RequestEventArgs e)
        {
            if (e.Url.EndsWith(".png"))
            {
                e.Async(req =>
                {
                    Thread.Sleep(3000);
                    Console.WriteLine(req.State);
                }, e.Url);
            }
        }

        private void View_DidCreateScriptContext(object sender, DidCreateScriptContextEventArgs e)
        {

        }

        private void View_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            View.LoadUri("http://www.easteat.com");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            View.LoadUri("https://www.baidu.com");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            View.Reload();
        }

        [JsFunc]
        private int add(int n1, int n2)
        {
            return n1 * n2;
        }

        [JsFunc]
        private async Task<string> showForm()
        {
            var t = Task.Factory.StartNew(() =>
            {
                Console.WriteLine("sleep begin");
                Thread.Sleep(3000);
                Console.WriteLine("sleep end");
            });
            Console.WriteLine("first");
            await t;
            Console.WriteLine("running");
            return "666";
        }
    }
}
