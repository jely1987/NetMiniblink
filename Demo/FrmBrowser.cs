using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QQ2564874169.Miniblink;
using QQ2564874169.Miniblink.LoadResourceImpl;
using QQ2564874169.Miniblink.LocalHttp;

namespace Demo
{
	public partial class FrmBrowser : Form
	{
		public FrmBrowser()
		{
			InitializeComponent();

            mbbw.LoadResourceHandlerList.Add(new LoadResourceByFile(
                Path.Combine(Application.StartupPath, "webres"), 
                "loc.webres"));
        }

		private void FrmTestBrowser_Load(object sender, EventArgs e)
		{
			//指定了本地站点后，所有文件加载方式都和web中一致
			mbbw.LoadUri("http://loc.webres/control.html");
            mbbw.AllowDrop = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mbbw.LoadUri(textBox1.Text);
        }

        private void mbbw_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
            Console.WriteLine(e);
        }

        private void mbbw_DragDrop(object sender, DragEventArgs e)
        {
            var items = (Array) e.Data.GetData(DataFormats.FileDrop);
            var files = items.Cast<string>().ToArray();
            var p = PointToClient(new Point(e.X, e.Y));
            mbbw.OnDropFiles(p.X, p.Y, files);
        }
    }
}
