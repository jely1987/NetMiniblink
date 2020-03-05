using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QQ2564874169.Miniblink;
using QQ2564874169.Miniblink.ResourceLoader;

namespace Demo
{
    public partial class FrmZipLoad : MiniblinkForm
    {
        public FrmZipLoad()
        {
            InitializeComponent();
            ResourceLoader.Add(new ZipLoader(
                Assembly.GetExecutingAssembly(),
                "/Demo/Res/zipdemo.zip",
                "loc.web"));
        }

        private void FrmZipLoad_Load(object sender, EventArgs e)
        {
            LoadUri("http://loc.web/demo.html");
        }
    }
}
