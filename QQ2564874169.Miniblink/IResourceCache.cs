﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QQ2564874169.Miniblink
{
    public interface IResourceCache
    {
        bool Matchs(string url, string mime);

        byte[] Get(string url);

        void Save(string url, byte[] data);
    }
}
