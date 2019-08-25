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
	public partial class FrmTransparent : MiniblinkForm
	{
		public FrmTransparent() : base(true)
		{
			InitializeComponent();
            LoadResourceHandlerList.Add(new LoadResourceByFile(
                Path.Combine(Application.StartupPath, "webres"),
                "loc.webres"));
        }

		private void FrmTransparent_Load(object sender, EventArgs e)
		{
			LoadUri("http://loc.webres/index.html");
		}
	}
}
