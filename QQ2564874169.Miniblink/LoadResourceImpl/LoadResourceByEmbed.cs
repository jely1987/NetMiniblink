﻿using System;
using System.Reflection;

namespace QQ2564874169.Miniblink.LoadResourceImpl
{
    public class LoadResourceByEmbed : ILoadResource
    {
        private Assembly _assembly;
        private string _dir;
        private string _namespace;
        private string _domain;

        public LoadResourceByEmbed(Assembly resAssembly, string resDir, string domain)
        {
            _domain = domain;
            _assembly = resAssembly;
            _dir = resDir;
            _namespace = resAssembly.EntryPoint.DeclaringType?.Namespace;
        }

        public byte[] ByUri(Uri uri)
        {
            if (string.Equals(uri.Host, _domain, StringComparison.OrdinalIgnoreCase) == false)
                return null;

            var path = string.Join(".", _namespace, _dir, uri.AbsolutePath.TrimStart('/').Replace("/", "."));

            using (var sm = _assembly.GetManifestResourceStream(path))
            {
                if (sm == null)
                {
                    return null;
                }

                var data = new byte[sm.Length];
                sm.Read(data, 0, data.Length);
                return data;
            }
        }
    }
}