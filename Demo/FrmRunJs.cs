using System;
using QQ2564874169.Miniblink;
using QQ2564874169.Miniblink.ResourceLoader;

namespace Demo
{
    public partial class FrmRunJs : MiniblinkForm
    {
        public FrmRunJs()
        {
            InitializeComponent();
            ResourceLoader.Add(new EmbedLoader(typeof(FrmMain).Assembly, "Res", "loc.res"));
        }

        private void FrmRunJs_Load(object sender, EventArgs e)
        {
            LoadUri("http://loc.res/runjs.html");
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var data = RunJs("return document.getElementById('ul').children.length");
            var count = Convert.ToInt32(data);
            RunJs($"document.getElementById('ul').innerHTML+='<li>{count + 1}</li>'");
        }
    }
}
