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
            //View.ResourceLoader.Add(new EmbedLoader(typeof(FrmMain).Assembly, "Res", "loc.res"));
        }

        private void FrmTest_Load(object sender, EventArgs e)
        {
            //View.ConsoleMessage += View_ConsoleMessage;
            //View.DidCreateScriptContext += View_DidCreateScriptContext;
            //View.RequestBefore += View_RequestBefore;
            View.LoadUri("https://www.baidu.com");
        }

        private void View_RequestBefore(object sender, RequestEventArgs e)
        {
            Console.WriteLine(e.Url);
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
        private void showForm()
        {
            var form = new MiniblinkForm();
            form.Load += (s, e) =>
            {
                //form.View.LoadUri("https://www.baidu.com");
            };
            form.ShowDialog();
        }
    }
}
