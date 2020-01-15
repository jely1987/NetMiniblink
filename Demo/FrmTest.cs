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

namespace Demo
{
    public partial class FrmTest : MiniblinkForm
    {
        public FrmTest()
        {
            InitializeComponent();
        }

        private void FrmTest_Load(object sender, EventArgs e)
        {
            NetResponse += FrmTest_NetResponse;
            LoadUrlBegin += FrmTest_LoadUrlBegin;
            LoadUri("https://www.bilibili.com");
        }

        private void FrmTest_LoadUrlBegin(object sender, LoadUrlBeginEventArgs e)
        {
            Console.WriteLine("begin : "+e.RequestMethod + " = " + e.Url);
        }

        private void FrmTest_NetResponse(object sender, NetResponseEventArgs e)
        {
            Console.WriteLine("resp : "+e.RequestMethod + " = " + e.Url);
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
    }
}
