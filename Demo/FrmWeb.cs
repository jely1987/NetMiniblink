﻿using System;
using QQ2564874169.Miniblink;
using QQ2564874169.Miniblink.LoadResourceImpl;
using QQ2564874169.Miniblink.LocalHttp;

namespace Demo
{
    public partial class FrmWeb : MiniblinkForm
    {
        private static NetApiEngine engine = new NetApiEngine();

        public FrmWeb()
        {
            InitializeComponent();
            LoadResourceHandlerList.Add(new EmbedLoader(typeof(FrmMain).Assembly, "Res", "loc.res"));
            ConsoleMessage += FrmWeb_ConsoleMessage;
        }

        private void FrmWeb_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private void FrmWeb_Load(object sender, EventArgs e)
        {
            LoadUri("http://loc.res/web.html");
        }

        [NetFunc("webapi")]
        private object GetUrl(string path)
        {
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }
            return engine.Domain + "/" + path;
        }
    }

    public class TimeApi : NetApi
    {
        [Get("/curr_time")]
        public string GetTime()
        {
            var min = Request.Query("min");
            var time = DateTime.Now.AddMinutes(Convert.ToInt32(min));
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }

        [Post("/save_time")]
        public string SaveTime()
        {
            var time = Request.Form("time");
            var zone = Request.Form("zone");
            return $"已保存{zone}时区的时间：{time}";
        }
    }
}
