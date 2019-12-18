using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QQ2564874169.Miniblink;

namespace Demo
{
    public partial class FrmDownload : MiniblinkForm
    {
        public FrmDownload()
        {
            InitializeComponent();
        }

        private void FrmDownload_Load(object sender, EventArgs e)
        {
            Text = "";
            Download += FrmDownload_Download;
            LoadUri("https://im.qq.com/pcqq/");
        }

        private void FrmDownload_Download(object sender, DownloadEventArgs e)
        {
            e.Progress += DownloadProgress;
            e.Finish += DownloadFinish;
        }

        private void DownloadFinish(object sender, DownloadFinshEventArgs e)
        {
            MessageBox.Show("下载完成");
            Text = "";
        }

        private void DownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            Text = $"下载中...{(int)(e.Received * 1.0 / e.Total * 100)}%";
        }
    }
}
