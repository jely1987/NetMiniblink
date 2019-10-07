using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QQ2564874169.Miniblink;
using QQ2564874169.Miniblink.LoadResourceImpl;

namespace Demo
{
    public partial class FrmWindow : MiniblinkForm
    {
        public FrmWindow()
        {
            InitializeComponent();

            LoadResourceHandlerList.Add(new LoadResourceByFile(
                Path.Combine(Application.StartupPath, "webres"),
                "loc.webres"));

            BindNetFunc(new NetFunc("test1", Test1));
	        BindNetFunc(new NetFunc("test2", Test2));
	        BindNetFunc(new NetFunc("test3", Test3));
	        BindNetFunc(new NetFunc("test4", Test4));
	        BindNetFunc(new NetFunc("test5", Test5));

	        var btn1 = new Button
	        {
		        Dock = DockStyle.Bottom,
		        Text = "调用Js函数"
	        };
	        btn1.Click += Btn1_Click;
	        Controls.Add(btn1);

	        var btn2 = new Button
	        {
		        Dock = DockStyle.Bottom,
		        Text = "开发者工具(路径不能有中文)"
	        };
	        btn2.Click += Btn2_Click;
	        Controls.Add(btn2);

            //允许在无边框模式下调整窗体大小
            NoneBorderResize = true;
			//设置边框阴影
			ShadowWidth.SetAll(3);
            //注册一个js函数，方便在页面里调用NetApi
            BindNetFunc(new NetFunc("netapi", ctx => FrmMain.NetApi.Domain + "/" + ctx.Paramters[0]));
		}

        private void Btn2_Click(object sender, EventArgs e)
        {
	        ShowDevTools();
        }

        private void Btn1_Click(object sender, EventArgs e)
        {
	        CallJsFunc("showAlert", "计算结果", 50, new TempNetFunc(param =>
            {
                var num = (double) param[0];
                MessageBox.Show("NET结果：" + (num * 2));
                return null;
            }));
        }

        private object Test1(NetFuncContext context)
        {
            var msg = string.Join(",", context.Paramters);
            MessageBox.Show(msg);
            return null;
        }

        private object Test2(NetFuncContext context)
        {
            return "参数是：" + context.Paramters[0];
        }

        private object Test3(NetFuncContext context)
        {
            var msg = $"test3参数是：{context.Paramters[0]}";

            return new TempNetFunc(callParam =>
            {
                msg += ", 之后的附加数据是：" + callParam[0];
                return msg;
            });
        }

        private object Test4(NetFuncContext context)
        {
            dynamic func = context.Paramters[0];

            Task.Run(() =>
            {
                Task.Delay(1000).Wait();

                func("abc", 12345, DateTime.Now);
            });

            return null;
        }

        private object Test5(NetFuncContext context)
        {
            var msg = new List<string>();

            foreach (dynamic obj in context.Paramters)
            {
                msg.Add($"id={obj.id}, name={obj.name}");
            }

            MessageBox.Show(string.Join("\n", msg));

            return null;
        }

        [NetFunc]
        public object Test6(int age, string name)
        {
            return $"{name}已经{age}岁了。";
        }

        [NetFunc]
        public object Test7(int age, string name, bool? sex = null, string addr = null)
        {
            var msg = $"{name}已经{age}岁了";
            if (sex.HasValue)
            {
                msg += $",{(sex.Value ? "男" : "女")}性";
            }
            else
            {
                msg += ",未知性别";
            }
            if (addr != null)
            {
                msg += ",所在地区:" + addr;
            }
            else
            {
                msg += ",未知地区";
            }
            return msg + "。";
        }

        [NetFunc("NewName")]
        public void Test8(int num, JsFunc func)
        {
            var result = func(num + 1);
            MessageBox.Show(result.ToString());
        }

		[NetFunc]
	    public void opensdk()
		{
			Process.Start("https://gitee.com/aochulai/NetMiniblink");
		}


        private void FrmTest_Load(object sender, EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.None;
            ConsoleMessage += FrmTest_ConsoleMessage;
            LoadUrlBegin += FrmTest_LoadUrlBegin;
            AlertBefore += FrmWindow_AlertBefore;
            ConfirmBefore += FrmWindow_ConfirmBefore;
            PromptBefore += FrmWindow_PromptBefore;

            //指定了本地站点后，所有文件加载方式都和web中一致
            LoadUri("http://loc.webres/index.html");
            //LoadUri("https://jbaysolutions.github.io/vue-grid-layout/examples/01-basic.html");
            //LoadUri("https://www.baidu.com");
            //LoadUri("https://www.cnblogs.com/wangkongming/p/6195903.html");
            //LoadUri("https://myliang.github.io/x-spreadsheet/");
            //LoadUri("https://www.qq.com");
            //LoadUri("https://www.acfun.cn");
        }

        private void FrmWindow_PromptBefore(object sender, PromptEventArgs e)
        {

        }

        private void FrmWindow_ConfirmBefore(object sender, ConfirmEventArgs e)
        {

        }

        private void FrmWindow_AlertBefore(object sender, AlertEventArgs e)
        {

        }

        private void FrmTest_LoadUrlBegin(object sender, LoadUrlBeginEventArgs e)
        {
            //以下全部是替换指定资源的返回结果
			if (e.Url.Contains("hook.js"))
			{
				e.Data = Encoding.UTF8.GetBytes("function hook(){alert('hook')}");
			}
			if (e.Url.Contains("hook2.js"))
			{
				e.WatchLoadUrlEnd(args =>
				{
					args.ReplaceData(Encoding.UTF8.GetBytes("function hook2(){alert('hook2')}"));
				});
			}
			if (e.Url.Contains("hook3.js"))
            {
                //wait方法是异步的，从文档来看是不卡UI线程的，但我实际测试只对异步ajax有效。
                e.Job.Wait(job =>
                {
                    job.Data = Encoding.UTF8.GetBytes("function hook3(){alert('hook3')}");
                });
            }
			if (e.Url.Contains("hook4.js"))
			{
				e.Job.Wait(job =>
				{
					job.WatchLoadUrlEnd(args =>
					{
						args.ReplaceData(Encoding.UTF8.GetBytes("function hook4(){alert('hook4')}"));
					});
				});
			}
		}

        private void FrmTest_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
