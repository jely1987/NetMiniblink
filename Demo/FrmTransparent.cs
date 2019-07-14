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

namespace Demo
{
	public partial class FrmTransparent : MiniblinkForm
	{
		public FrmTransparent() : base(true)
		{
			InitializeComponent();
		}

		private void FrmTransparent_Load(object sender, EventArgs e)
		{
			SetLocalResource(Path.Combine(Application.StartupPath, "webres"), "loc.webres");
			LoadUri("/index.html");
		}
	}
}
