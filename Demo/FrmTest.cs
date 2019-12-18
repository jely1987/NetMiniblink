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
            Download += FrmTest_Download;
            LoadUri("https://im.qq.com/pcqq/");
        }

        private void FrmTest_Download(object sender, DownloadEventArgs e)
        {
            e.Progress += E_Progress;
            e.SaveToFile("111.exe");
        }

        private void E_Progress(object sender, DownloadProgressEventArgs e)
        {
            Console.WriteLine(e.Total + "\t" + e.Received);
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
