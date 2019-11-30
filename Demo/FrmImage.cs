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
    public partial class FrmImage : MiniblinkForm
    {
        public FrmImage()
        {
            InitializeComponent();
        }

        private void FrmImage_Load(object sender, EventArgs e)
        {
            LoadUri("https://gitee.com/aochulai/NetMiniblink/raw/master/README.md");
        }

        private void btnImage_Click(object sender, EventArgs e)
        {
            DrawToBitmap(arg =>
            {
                arg.GetImage().Save(Guid.NewGuid() + ".png");
                MessageBox.Show("截图已保存");
            });
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            Print(dialog => { dialog.ShowDialog(); });
        }
    }
}
