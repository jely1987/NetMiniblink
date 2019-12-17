using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace QQ2564874169.Miniblink
{
    public class CookieCollection : IList<Cookie>
    {
        public int Count
        {
            get { return _container.Count; }
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
                if (_enable != value)
                {
                    if (value)
                    {
                        _miniblink.LoadUrlBegin -= ClearCookie;
                    }
                    else
                    {
                        _container = new CookieContainer();
                        _miniblink.LoadUrlBegin += ClearCookie;
                        MBApi.wkePerformCookieCommand(_miniblink.MiniblinkHandle, 
                            wkeCookieCommand.FlushCookiesToFile);
                    }
                }

                _enable = value;
            }
        }

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
            _miniblink.NavigateBefore += _miniblink_NavigateBefore;
            Enable = true;
        }

        private void ClearCookie(object sender, LoadUrlBeginEventArgs e)
        {
            MBApi.wkePerformCookieCommand(_miniblink.MiniblinkHandle, wkeCookieCommand.ClearAllCookies);
            MBApi.wkePerformCookieCommand(_miniblink.MiniblinkHandle, wkeCookieCommand.ClearSessionCookies);
            MBApi.wkePerformCookieCommand(_miniblink.MiniblinkHandle, wkeCookieCommand.ReloadCookiesFromFile);
        }

        private void _miniblink_NavigateBefore(object sender, NavigateEventArgs e)
        {
            if (Enable == false) return;
            if (_miniblink.Url == e.Url)
            {
                _container = new CookieContainer();
            }
        }

        private void Reload()
        {
            MBApi.wkePerformCookieCommand(_miniblink.MiniblinkHandle, wkeCookieCommand.FlushCookiesToFile);
            _container = new CookieContainer();
            var host = new Uri(_miniblink.Url).Host;
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
                    if (host.EndsWith(domain) || ("." + host).Equals(domain) == false)
                    {
                        continue;
                    }
                }
                else if (host.Equals(domain) == false)
                {
                    continue;
                }

                var cookie = new Cookie
                {
                    HttpOnly = httpOnly,
                    Domain = domain.TrimStart('.'),
                    Path = items[2],
                    Secure = "true".Equals(items[3], StringComparison.OrdinalIgnoreCase),
                    Expires = new DateTime(1970, 1, 1).AddSeconds(long.Parse(items[4])),
                    Name = items[5],
                    Value = Utils.UrlEncode(items[6])
                };
                _container.Add(cookie);
            }
        }

        private IList<Cookie> GetCookies()
        {
            Reload();
            var list = new List<Cookie>();
            foreach (Cookie item in _container.GetCookies(new Uri(_miniblink.Url)))
            {
                if (list.Contains(item) == false)
                {
                    list.Add(item);
                }
            }

            return list.AsReadOnly();
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
            MBApi.wkeSetCookie(_miniblink.MiniblinkHandle, GetCurlCookie(cookie));
            _container.Add(cookie);
        }

        public void Clear()
        {
            var ck = "";
            foreach (var cookie in GetCookies())
            {
                cookie.Expires = DateTime.MinValue;
                ck+= GetCurlCookie(cookie);
            }

            MBApi.wkeSetCookie(_miniblink.MiniblinkHandle, ck);
            MBApi.wkePerformCookieCommand(_miniblink.MiniblinkHandle, wkeCookieCommand.FlushCookiesToFile);
            _container = new CookieContainer();
        }

        public bool Contains(Cookie cookie)
        {
            if (cookie == null) return false;
            
            if (string.IsNullOrEmpty(cookie.Path))
            {
                cookie.Path = "/";
            }

            if (string.IsNullOrEmpty(cookie.Domain))
            {
                cookie.Domain = new Uri(_miniblink.Url).Host;
            }
            var list = _container.GetCookies(new Uri("http://" + cookie.Domain + cookie.Path));
            foreach (Cookie item in list)
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

            if (string.IsNullOrEmpty(cookie.Path))
            {
                cookie.Path = "/";
            }

            if (string.IsNullOrEmpty(cookie.Domain))
            {
                cookie.Domain = new Uri(_miniblink.Url).Host;
            }

            if (Contains(cookie))
            {
                cookie.Expires = DateTime.MinValue;
                var ck = GetCurlCookie(cookie);
                MBApi.wkeSetCookie(_miniblink.MiniblinkHandle, ck);
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
