using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo
{
    public partial class FrmPrint : Form
    {
        private Image jietu;

        public FrmPrint()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //var ppd = new PrintPreviewDialog();
            //ppd.Document = printDocument1;
            //ppd.ShowDialog();
            //return;

            //miniblinkBrowser1.PrintToBitmap(img =>
            //{
            //    var name = Guid.NewGuid() + ".png";
            //    img.Save(name);
            //    jietu = Image.FromFile(name);

            //    if (printDialog1.ShowDialog() == DialogResult.OK)
            //    {
            //        printDocument1.Print();
            //    }
            //});
            miniblinkBrowser1.PrintToSm(sm =>
            {
                Image.FromStream(sm).Save(Guid.NewGuid().ToString() + ".png");
            });
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            //e.Graphics.DrawImage(jietu, 0, 0, jietu.Width, jietu.Height);
        }

        private void FrmPrint_Load(object sender, EventArgs e)
        {
            miniblinkBrowser1.LoadUri("https://www.qq.com");
        }
    }
}
