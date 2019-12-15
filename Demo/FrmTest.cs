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
using QQ2564874169.Miniblink.LoadResourceImpl;

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
            LoadUrlBegin += FrmTest_LoadUrlBegin;
            LoadUri("https://www.acfun.cn");
        }

        private void FrmTest_LoadUrlBegin(object sender, LoadUrlBeginEventArgs e)
        {
            e.WatchLoadUrlEnd(p => { Console.WriteLine(p.RequestMethod + " = " + p.Url); });
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
