using System;
using System.Linq;
using System.Windows.Forms;

namespace QQ2564874169.Miniblink
{
    public delegate object JsFunc(params object[] param);

    public class JsFuncWapper
    {
        private string _name;
        private IntPtr _mb;
        private Control _ctrl;

        internal JsFuncWapper(Control control, long jsvalue, IntPtr es)
        {
            _name = "func" + Guid.NewGuid().ToString().Replace("-", "");
            _mb = MBApi.jsGetWebView(es);
            MBApi.jsSetGlobal(es, _name, jsvalue);
            _ctrl = control;
        }

        public object Call(params object[] param)
        {
            object result = null;

            _ctrl.UIInvoke(() =>
			{
				var es = MBApi.wkeGlobalExec(_mb);
				var value = MBApi.jsGetGlobal(es, _name);
                var jsps = param.Select(i => i.ToJsValue(_ctrl, es)).ToArray();
                result = MBApi.jsCall(es, value, MBApi.jsUndefined(), jsps, jsps.Length).ToValue(_ctrl, es);
				MBApi.jsSetGlobal(es, _name, MBApi.jsUndefined());
			});
			
            return result;
        }
    }
}
