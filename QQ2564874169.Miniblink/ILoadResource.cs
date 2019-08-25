using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace QQ2564874169.Miniblink
{
    public interface ILoadResource
    {
        byte[] ByUri(Uri uri);
    }

    public class DefaultLoadResource : ILoadResource
    {
        private ResourceManager _manager;
        private string _root;
        private string _namespace;
        private Assembly _assembly;

        public DefaultLoadResource(Type resType, string root)
        {
            _assembly = resType.Assembly;
            _root = root ?? "";
            _namespace = resType.Namespace;
            _manager = new ResourceManager(resType)
            {
                IgnoreCase = true
            };
        }

        public byte[] ByUri(Uri uri)
        {
            var path =  uri.AbsolutePath.TrimStart('/');
            path = _namespace + "/" + _root + "/" + path;
            path = path.Replace("/", ".");

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
