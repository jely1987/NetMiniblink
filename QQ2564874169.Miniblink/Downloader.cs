using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace QQ2564874169.Miniblink
{
    public class Downloader
    {
        private DownloadEventArgs _args;
        private IMiniblink _miniblink;
        private bool _cancel;

        internal Downloader(IMiniblink miniblink, DownloadEventArgs args)
        {
            _args = args;
            _miniblink = miniblink;
            _miniblink.Destroy += _miniblink_Destroy;
        }

        private void _miniblink_Destroy(object sender, EventArgs e)
        {
            _cancel = true;
            _miniblink.Destroy -= _miniblink_Destroy;
        }

        public void Start()
        {
            var cookies = _miniblink.Cookies.GetCookies(_args.Url);
            var http = (HttpWebRequest) WebRequest.Create(_args.Url);
            http.Method = "get";
            http.AllowAutoRedirect = true;
            http.UserAgent = _miniblink.UserAgent;
            foreach (var item in cookies)
            {
                http.CookieContainer.Add(item);
            }

            Task.Factory.StartNew(() => { Do(http); }).ContinueWith(t =>
            {
                if (_cancel == false)
                {
                    _args.OnFinish(new DownloadFinshEventArgs
                    {
                        Error = t.Exception?.GetBaseException()
                    });
                }
            });
        }

        private void Do(WebRequest http)
        {
            var resp = http.GetResponse();
            var total = resp.ContentLength;
            var rec = 0L;
            using (var sm = resp.GetResponseStream())
            {
                using (var buf = new MemoryStream(1024 * 1024 * 2))
                {
                    int len;
                    var log = DateTime.Now.Ticks;
                    var data = new byte[1024 * 512];
                    while ((len = sm.Read(data, 0, data.Length)) > 0 && _cancel == false)
                    {
                        buf.Write(data, 0, len);
                        rec += len;
                        if (rec > total)
                        {
                            throw new Exception("数据异常，无法下载");
                        }

                        if (new TimeSpan(DateTime.Now.Ticks - log).TotalSeconds >= 1 || rec == total)
                        {
                            var pres = new DownloadProgressEventArgs
                            {
                                Total = total,
                                Received = rec,
                                Data = buf.ToArray()
                            };
                            _miniblink.SafeInvoke(s => { _args.OnProgress((DownloadProgressEventArgs) s); }, pres);
                            buf.SetLength(0);
                            buf.Position = 0;
                            log = DateTime.Now.Ticks;
                            if (pres.Cancel)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
