using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QQ2564874169.Miniblink
{
    public static class MiniblinkSetting
    {
        public static void EnableHighDPISupport()
        {
            MBApi.wkeEnableHighDPISupport();
        }

        public static void SetProxy(WKEProxy proxy)
        {
            MBApi.wkeSetProxy(proxy);
        }
    }
}
