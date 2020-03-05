using System;
using QQ2564874169.Miniblink;
using QQ2564874169.Miniblink.ResourceLoader;

namespace Demo
{
    public partial class FrmEvents : MiniblinkForm
    {
        public FrmEvents()
        {
            InitializeComponent();
            ResourceLoader.Add(new EmbedLoader(typeof(FrmMain).Assembly, "Res", "loc.res"));
        }

        private void FrmEvents_Load(object sender, EventArgs e)
        {
            LoadUri("http://loc.res/events.html");
        }
    }
}
