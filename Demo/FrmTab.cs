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
    public partial class FrmTab : MiniblinkForm
    {
        private string dir = Path.Combine(Application.StartupPath, "webres");

        public FrmTab()
        {
            InitializeComponent();
            LoadResourceHandlerList.Add(new LoadResourceByFile(dir, "loc.web"));
        }

        private void FrmTab_Load(object sender, EventArgs e)
        {
            NavigateBefore += FrmTab_NavigateBefore;
            DocumentReady += FrmTab_DocumentReady;
            LoadUri("http://loc.web/nav.html");
        }

        private void FrmTab_DocumentReady(object sender, DocumentReadyEventArgs e)
        {
            e.Frame.InsertCss("<style>a:{color:red}</style>");
        }

        private void FrmTab_NavigateBefore(object sender, QQ2564874169.Miniblink.NavigateEventArgs e)
        {
            Console.WriteLine(e.Type);
        }
    }
}
