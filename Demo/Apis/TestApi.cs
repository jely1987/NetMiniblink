using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QQ2564874169.Miniblink;
using QQ2564874169.Miniblink.LocalHttp;

namespace Demo.Apis
{
    public class TestApi : NetApi
    {
        [Get]
        public string Get()
        {
            var id = Request.Query("id");
            var name = Request.Query("name");

            return "Get >> 拼接之后：" + id + " , " + name;
        }

        [Post("/changeurl")]
        public string Post()
        {
            var id = Request.Form("id");
            var name = Request.Form("name");

            return "Post >> 拼接之后：" + id + " , " + name;
        }
    }
}
