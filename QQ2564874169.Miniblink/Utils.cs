using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace QQ2564874169.Miniblink
{
    internal static class Utils
    {
        public static int LOWORD(IntPtr dword)
        {
            return (int) dword & 65535;
        }

        public static int HIWORD(IntPtr dword)
        {
            return (int) dword >> 16;
        }

        public static IntPtr Dword(IntPtr dword)
        {
            return new IntPtr((IntPtr.Size == 8) ? (int) (dword.ToInt64() << 32 >> 32) : dword.ToInt32());
        }

        public static string[] PtrToStringArray(IntPtr ptr, int length)
        {
            var data = new string[length];

            for (var i = 0; i < length; ++i)
            {
                var str = (IntPtr) Marshal.PtrToStructure(ptr, typeof(IntPtr));
                data[i] = Marshal.PtrToStringAnsi(str);
                ptr = new IntPtr(ptr.ToInt64() + IntPtr.Size);
            }
            return data;
        }

        public static wkePostBodyElement[] PtrToPostElArray(IntPtr ptr, int length)
        {
            var data = new wkePostBodyElement[length];

            for (var i = 0; i < length; ++i)
            {
                var tmp = (IntPtr) Marshal.PtrToStructure(ptr, typeof(IntPtr));
                data[i] = (wkePostBodyElement) Marshal.PtrToStructure(tmp, typeof(wkePostBodyElement));
                ptr = new IntPtr(ptr.ToInt64() + IntPtr.Size);
            }
            return data;
        }

	    public static bool IsDesignMode()
	    {
		    var returnFlag = false;

#if DEBUG
		    if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
		    {
			    returnFlag = true;
		    }
		    else if (Process.GetCurrentProcess().ProcessName == "devenv")
		    {
			    returnFlag = true;
		    }
#endif

		    return returnFlag;
	    }
	}
}
