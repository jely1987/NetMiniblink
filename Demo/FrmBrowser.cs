using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QQ2564874169.Miniblink;
using QQ2564874169.Miniblink.LocalHttp;

namespace Demo
{
	public partial class FrmBrowser : Form
	{
		public FrmBrowser()
		{
			InitializeComponent();

			mbbw.SetLocalResource(Path.Combine(Application.StartupPath, "webres"), "loc.webres");

			mbbw.BindNetFunc(new NetFunc("test1", Test1));
			mbbw.BindNetFunc(new NetFunc("test2", Test2));
			mbbw.BindNetFunc(new NetFunc("test3", Test3));
			mbbw.BindNetFunc(new NetFunc("test4", Test4));
			mbbw.BindNetFunc(new NetFunc("test5", Test5));
            //注册一个js函数，方便在页面里调用NetApi
            mbbw.BindNetFunc(new NetFunc("netapi", ctx => FrmMain.NetApi.Domain + "/" + ctx.Paramters[0]));
        }

		private void FrmTestBrowser_Load(object sender, EventArgs e)
		{
			mbbw.LoadUrlBegin += Mbbw_LoadUrlBegin;

			//指定了本地站点后，所有文件加载方式都和web中一致
			mbbw.LoadUri("/index.html");
		}

		private void Mbbw_LoadUrlBegin(object sender, LoadUrlBeginEventArgs e)
		{
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
				e.Job.Wait(job =>
				{
					job.Data = Encoding.UTF8.GetBytes("function hook3(){alert('hook3')}");
				});
			}
			if (e.Url.Contains("hook4.js"))
			{
				e.Job.Wait(job =>
				{
					e.WatchLoadUrlEnd(args =>
					{
						args.ReplaceData(Encoding.UTF8.GetBytes("function hook4(){alert('hook4')}"));
					});
					Task.Delay(1).Wait();
				});
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			mbbw.CallJsFunc("showAlert", "计算结果", 50, new TempNetFunc(param =>
			{
				var num = (double) param[0];
				MessageBox.Show("NET结果：" + (num * 2));
				return null;
			}));
		}

		private void button2_Click(object sender, EventArgs e)
		{
			mbbw.ShowDevTools();
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
	}
}
