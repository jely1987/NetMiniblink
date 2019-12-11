using System;
using System.Collections;
using System.Collections.Generic;
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

        private List<string> _hosts = new List<string>();
        private CookieContainer _container = new CookieContainer();
        private IMiniblink _miniblink;
        private string _file;

        internal CookieCollection(IMiniblink miniblink, string path)
        {
            _file = path;
            _miniblink = miniblink;
            _miniblink.LoadUrlBegin += _miniblink_LoadUrlBegin;
            _miniblink.NavigateBefore += _miniblink_NavigateBefore;
        }

        private void _miniblink_NavigateBefore(object sender, NavigateEventArgs e)
        {
            if (_miniblink.Url == e.Url)
            {
                _container = new CookieContainer();
                _hosts.Clear();
            }
        }

        private void _miniblink_LoadUrlBegin(object sender, LoadUrlBeginEventArgs e)
        {
            var uri = new Uri(e.Url);
            if (_hosts.Contains(uri.Host) == false)
            {
                _hosts.Add(uri.Host);
            }
        }

        private void Reload()
        {

        }

        private List<Cookie> GetCookies()
        {
            Reload();
            var list = new List<Cookie>();
            foreach (var host in _hosts)
            {
                foreach (Cookie item in _container.GetCookies(new Uri(host)))
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
