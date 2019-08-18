using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QQ2564874169.Miniblink;

namespace Demo
{
    public partial class FrmPrint : Form
    {
        public FrmPrint()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            miniblinkBrowser1.Print(dialog =>
            {
                //如有需要，可以先设置一下
                dialog.StartPosition = FormStartPosition.CenterScreen;
                dialog.ShowDialog();
            });
        }

        private void FrmPrint_Load(object sender, EventArgs e)
        {
            miniblinkBrowser1.DocumentReady += MiniblinkBrowser1_DocumentReady;
            miniblinkBrowser1.NavigateBefore += MiniblinkBrowser1_NavigateBefore;
            miniblinkBrowser1.LoadUri("https://www.qq.com");
        }

        private void MiniblinkBrowser1_NavigateBefore(object sender, QQ2564874169.Miniblink.NavigateEventArgs e)
        {
            button1.Enabled = false;
        }

        private void MiniblinkBrowser1_DocumentReady(object sender, DocumentReadyEventArgs e)
        {
            button1.Enabled = true;
            Text = miniblinkBrowser1.DocumentTitle;
        }
    }
}
