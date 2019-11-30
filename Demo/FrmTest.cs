using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QQ2564874169.Miniblink;
using QQ2564874169.Miniblink.LoadResourceImpl;

namespace Demo
{
    public partial class FrmTest : MiniblinkForm
    {
        private string dir = Path.Combine(Application.StartupPath, "webres");

        public FrmTest()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.None;
            NoneBorderResize = true;
            LoadResourceHandlerList.Add(new FileLoader(dir, "loc.web"));
        }

        private void FrmTest_Load(object sender, EventArgs e)
        {
            LoadUri("http://loc.web/test.html");
            var pnl = new Panel
            {
                Width = Width / 2,
                Height = 30,
                Left = 30,
                Top = 30
            };
            Controls.Add(pnl);
            pnl.BringToFront();
        }
    }
}
