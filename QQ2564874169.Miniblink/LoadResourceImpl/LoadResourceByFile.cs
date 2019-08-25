using System;
using System.IO;

namespace QQ2564874169.Miniblink.LoadResourceImpl
{
    public class LoadResourceByFile : ILoadResource
    {
        private string _domain;
        private string _dir;

        public LoadResourceByFile(string dir, string domain)
        {
            _dir = dir.TrimEnd('/');
            _domain = domain.TrimEnd('/');
        }

        public byte[] ByUri(Uri uri)
        {
            if (string.Equals(uri.Host, _domain, StringComparison.OrdinalIgnoreCase) == false)
                return null;
            var path = _dir + uri.AbsolutePath.Replace("/", Path.DirectorySeparatorChar.ToString());
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }
    }
}
