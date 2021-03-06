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
using QQ2564874169.Miniblink.ResourceLoader;

namespace Demo
{
    public partial class FrmJsCallNet : MiniblinkForm
    {
        public FrmJsCallNet()
        {
            InitializeComponent();
            View.ResourceLoader.Add(new EmbedLoader(typeof(FrmMain).Assembly, "Res", "loc.res"));
        }

        private void FrmJsCallNet_Load(object sender, EventArgs e)
        {
            View.LoadUri("http://loc.res/js_call_net.html");
        }

        [JsFunc]
        private object Func1(int n1, int n2)
        {
            return "结果是：" + (n1 * n2);
        }

        [JsFunc]
        private void Func2(JsFunc func)
        {
            func(5, 6);
        }

        [JsFunc]
        private object Func3(dynamic data)
        {
            return data.n1 * data.n2;
        }

        [JsFunc]
        private object Func4(string name, int age, int? year)
        {
            return $"name={name}, age={age}, year={year}";
        }

        [JsFunc]
        private object Func5()
        {
            return new TempNetFunc(param => "姓名：" + param[0]);
        }
    }
}
