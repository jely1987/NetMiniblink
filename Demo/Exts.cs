using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo
{
    internal static class Exts
    {
        public static void SafeInvoke(this Control control, Action callback)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(callback);
            }
            else
            {
                callback();
            }
        }
    }
}
