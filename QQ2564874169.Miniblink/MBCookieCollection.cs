using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace QQ2564874169.Miniblink
{
    public class CookieCollection : IList<Cookie>
    {
        public int Count
        {
            get { return GetCookies().Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        private bool _enable;

        public bool Enable
        {
            get { return _enable; }
            set
            {
                if (_enable != value && value == false)
                {
                    _hosts.Clear();
                    _container = new CookieContainer();
                }

                _enable = value;
            }
        }

        private List<string> _hosts = new List<string>();
        private CookieContainer _container = new CookieContainer();
        private IMiniblink _miniblink;
        private string _file;

        internal CookieCollection(IMiniblink miniblink, string path)
        {
            if (File.Exists(path) == false)
            {
                throw new FileNotFoundException("cookies文件不存在", path);
            }
            _file = path;
            _miniblink = miniblink;
            _miniblink.LoadUrlBegin += _miniblink_LoadUrlBegin;
            _miniblink.NavigateBefore += _miniblink_NavigateBefore;
            Enable = true;
        }

        private void _miniblink_NavigateBefore(object sender, NavigateEventArgs e)
        {
            if (Enable == false) return;
            if (_miniblink.Url == e.Url)
            {
                _container = new CookieContainer();
                _hosts.Clear();
            }
        }

        private void _miniblink_LoadUrlBegin(object sender, LoadUrlBeginEventArgs e)
        {
            if (Enable == false) return;
            var host = new Uri(e.Url).Host.ToLower();
            if (_hosts.Contains(host) == false)
            {
                _hosts.Add(host);
            }
        }

        private void Reload()
        {
            MBApi.wkePerformCookieCommand(_miniblink.MiniblinkHandle,wkeCookieCommand.FlushCookiesToFile);
            var rows = File.ReadAllLines(_file, Encoding.UTF8);
            foreach (var row in rows)
            {
                if (row.StartsWith("# ")) continue;
                var items = row.Split('\t');
                if (items.Length != 7) continue;
                var domain = items[0].ToLower();
                var httpOnly = domain.StartsWith("#HttpOnly_", StringComparison.OrdinalIgnoreCase);
                if (httpOnly)
                {
                    domain = domain.Substring(domain.IndexOf("_", StringComparison.Ordinal) + 1).ToLower();
                }

                if (domain.StartsWith("."))
                {
                    if (_hosts.Any(i => i.EndsWith(domain) || ("." + i).Equals(domain)) == false)
                    {
                        continue;
                    }
                }
                else if (_hosts.Contains(domain) == false)
                {
                    continue;
                }

                foreach (var v in items[6].Split(','))
                {
                    var cookie = new Cookie
                    {
                        HttpOnly = httpOnly,
                        Domain = domain.TrimStart('.'),
                        Path = items[2],
                        Secure = "true".Equals(items[3], StringComparison.OrdinalIgnoreCase),
                        Expires = new DateTime(1970, 1, 1).AddSeconds(long.Parse(items[4])),
                        Name = items[5],
                        Value = v
                    };
                    _container.Add(cookie);
                }
            }
        }

        private List<Cookie> GetCookies()
        {
            Reload();
            var list = new List<Cookie>();
            foreach (var host in _hosts)
            {
                foreach (Cookie item in _container.GetCookies(new Uri("http://" + host)))
                {
                    list.Add(item);
                }
            }

            return list;
        }

        private static string GetCurlCookie(Cookie cookie)
        {
            var ck = $"{cookie.Name}={cookie.Value};expires={cookie.Expires:R};domain={cookie.Domain};path={cookie.Path};";
            if (cookie.Secure)
            {
                ck += "secure;";
            }

            if (cookie.HttpOnly)
            {
                ck += "httponly;";
            }

            return ck;
        }

        public IEnumerator<Cookie> GetEnumerator()
        {
            return GetCookies().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Cookie cookie)
        {
            if (cookie == null) return;
            if (string.IsNullOrEmpty(cookie.Path))
            {
                cookie.Path = "/";
            }

            if (string.IsNullOrEmpty(cookie.Domain))
            {
                cookie.Domain = new Uri(_miniblink.Url).Host;
            }
            MBApi.wkeSetCookie(_miniblink.MiniblinkHandle, "http://" + cookie.Domain + cookie.Path, GetCurlCookie(cookie));
            _container.Add(cookie);
        }

        public void Clear()
        {
            foreach (var cookie in GetCookies())
            {
                var ck = GetCurlCookie(cookie);
                MBApi.wkeSetCookie(_miniblink.MiniblinkHandle, "http://" + cookie.Domain + cookie.Path, ck);
            }

            MBApi.wkePerformCookieCommand(_miniblink.MiniblinkHandle, wkeCookieCommand.FlushCookiesToFile);
            _container = new CookieContainer();
        }

        public bool Contains(Cookie item)
        {
            if (item == null) return false;

            var list = _container.GetCookies(new Uri("http://" + item.Domain + item.Path));
            foreach (Cookie cookie in list)
            {
                if (cookie.Name == item.Name && cookie.Value == item.Value && cookie.Path == item.Path)
                {
                    return true;
                }
            }

            return false;
        }

        public bool Remove(Cookie cookie)
        {
            if (cookie == null) return false;
            if (Contains(cookie))
            {
                cookie.Expires = DateTime.Now;
                var ck = GetCurlCookie(cookie);
                MBApi.wkeSetCookie(_miniblink.MiniblinkHandle, "http://" + cookie.Domain + cookie.Path, ck);
                return true;
            }

            return false;
        }

        public void CopyTo(Cookie[] array, int arrayIndex)
        {
            var list = GetCookies();
            for (var i = 0; i < list.Count && arrayIndex < array.Length; i++, arrayIndex++)
            {
                array[arrayIndex] = list[i];
            }
        }

        public void Insert(int index, Cookie item)
        {
            Add(item);
        }

        public int IndexOf(Cookie item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public Cookie this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
