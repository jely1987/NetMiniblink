using System;
using System.IO;
using System.Windows.Forms;
using QQ2564874169.Miniblink.LoadResourceImpl;
using System.Runtime.InteropServices;

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
			//mbbw.LoadUri("http://loc.webres/control.html");
            mbbw.LoadUri("https://www.qq.com");
            mbbw.AllowDrop = true;
            mbbw.FireDropFile = true;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var url = textBox1.Text;
            if (url.StartsWith("http") == false)
            {
                url = "http://" + url;
            }
            mbbw.LoadUri(url);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mbbw.DrawToBitmap(imgs =>
            {

            });
        }
    }
}
