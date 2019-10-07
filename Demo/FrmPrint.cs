using System;
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
            ShowMsg();
            miniblinkBrowser1.Print(dialog =>
            {
                //如有需要，可以先设置一下
                dialog.StartPosition = FormStartPosition.CenterScreen;
                dialog.ShowDialog();
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ShowMsg();
            miniblinkBrowser1.DrawToBitmap(s =>
            {
                using (var img = s.GetImage())
                {
                    img.Save(Guid.NewGuid() + ".png");
                    MessageBox.Show("截图已保存");
                }
            });
        }

        private void ShowMsg()
        {
            MessageBox.Show(@"实现方式是滚动截屏，为了稳定建议各位自己维护下目标内容。");
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
            button2.Enabled = false;
        }

        private void MiniblinkBrowser1_DocumentReady(object sender, DocumentReadyEventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = true;
            Text = miniblinkBrowser1.DocumentTitle;
        }
    }
}
