using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QQ2564874169.Miniblink.LocalHttp;

namespace Demo
{
	public partial class FrmMain : Form
	{
        public static NetApiEngine NetApi { get; private set; }

		public FrmMain()
		{
			InitializeComponent();

            if (NetApi == null)
            {
                //实例化时会扫描当前加载的程序集找出所有NetApi
                NetApi = new NetApiEngine();
            }
        }

		private void button1_Click(object sender, EventArgs e)
		{
			new FrmWindow().Show();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			new FrmBrowser().Show();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			new FrmTransparent().Show();
		}

        private void button4_Click(object sender, EventArgs e)
        {
            new FrmPrint().Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            new FrmPkg().Show();
        }
    }
}
