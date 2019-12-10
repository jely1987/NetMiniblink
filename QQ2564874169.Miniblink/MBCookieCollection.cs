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
            get { return _cookies.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        private IList<Cookie> _cookies = new List<Cookie>();
        private IMiniblink _miniblink;
        private string _file;

        internal CookieCollection(IMiniblink miniblink, string path)
        {
            _miniblink = miniblink;
            _file = path;
        }

        public IEnumerator<Cookie> GetEnumerator()
        {

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Cookie item)
        {

        }

        public void Clear()
        {
            _cookies.Clear();
        }

        public bool Contains(Cookie item)
        {
            return IndexOf(item) > -1;
        }

        public void CopyTo(Cookie[] array, int arrayIndex)
        {
            for (var i = 0; i < Count && arrayIndex < array.Length; i++, arrayIndex++)
            {
                array[arrayIndex] = _cookies[i];
            }
        }

        public bool Remove(Cookie item)
        {

        }

        public int IndexOf(Cookie item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, Cookie item)
        {
            Add(item);
        }

        public void RemoveAt(int index)
        {
            Remove(_cookies[index]);
        }

        public Cookie this[int index]
        {
            get { return _cookies[index]; }
            set
            {
                var item = _cookies[index];
                Remove(item);
                Add(value);
            }
        }

        private void Reload()
        {

        }
    }
}
