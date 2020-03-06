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
    public partial class FrmDevTools : MiniblinkForm
    {
        public FrmDevTools()
        {
            InitializeComponent();
        }

        private void FrmDevTools_Load(object sender, EventArgs e)
        {
            View.LoadUri("https://www.baidu.com");
        }

        private void btnDevTools_Click(object sender, EventArgs e)
        {
            View.ShowDevTools();
        }
    }
}
