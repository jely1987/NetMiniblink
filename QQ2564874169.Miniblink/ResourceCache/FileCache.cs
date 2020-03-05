using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QQ2564874169.Miniblink.ResourceCache
{
    public class FileCache : IResourceCache
    {
        public List<string> UrlRegex { get; }
        public List<string> MimeRegex { get; }
        public int SlidingMinute { get; set; }

        public FileCache()
        {
            SlidingMinute = 30;
            UrlRegex = new List<string>();
            MimeRegex = new List<string>
            {
                "image/*",
                "application/javascript",
                "text/css"
            };;
        }

        public bool Matchs(string mime, string url)
        {
            var success = false;

            if (MimeRegex.Count > 0)
            {
                foreach (var item in MimeRegex)
                {
                    if (Regex.IsMatch(mime, item, RegexOptions.IgnoreCase))
                    {
                        success = true;
                        break;
                    }
                }
            }

            if (UrlRegex.Count > 0 && success)
            {
                success = false;
                foreach (var item in UrlRegex)
                {
                    if (Regex.IsMatch(url, item, RegexOptions.IgnoreCase))
                    {
                        success = true;
                        break;
                    }
                }
            }

            return success;
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
