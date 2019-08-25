using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QQ2564874169.Miniblink.LoadResourceImpl;

namespace Demo
{
    public partial class FrmPkg : Form
    {
        public FrmPkg()
        {
            InitializeComponent();
            miniblinkBrowser1.LoadResourceHandlerList.Add(
                new LoadResourceByEmbed(
                    typeof(FrmMain).Assembly,
                    "Res",
                    "loc.res"));
        }

        private void FrmPkg_Load(object sender, EventArgs e)
        {
            miniblinkBrowser1.LoadUri("http://loc.res/pkg.html");
        }
    }
}
