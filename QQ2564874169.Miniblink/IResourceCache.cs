using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QQ2564874169.Miniblink
{
    public interface IResourceCache
    {
        byte[] Get(string url, string mime);
    }
}
