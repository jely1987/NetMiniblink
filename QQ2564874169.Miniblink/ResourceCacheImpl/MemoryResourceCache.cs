using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace QQ2564874169.Miniblink.ResourceCacheImpl
{
    public class MemoryResourceCache : IResourceCache
    {
        private MemoryCache _cache = new MemoryCache(Guid.NewGuid().ToString());
        public List<string> UrlRegex { get; }
        public List<string> MimeRegex { get; }

        public MemoryResourceCache()
        {
            UrlRegex = new List<string>();
            MimeRegex = new List<string>();
        }

        public bool Matchs(string mime, string url)
        {
            throw new NotImplementedException();
        }

        public byte[] Get(string url)
        {
            throw new NotImplementedException();
        }

        public void Save(string url, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
