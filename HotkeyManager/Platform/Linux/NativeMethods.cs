using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HotkeyManager.Platform.Linux
{
    public static class NativeMethods
    {
        [DllImport("libX11.so.6")]
        public static extern IntPtr XOpenDisplay(string display_name);
        [DllImport("libX11.so.6")]
        public static extern int XCloseDisplay(IntPtr display);
        [DllImport("libX11.so.6")]
        public static extern int XRaiseWindow(IntPtr display, IntPtr window);
        [DllImport("libX11.so.6")]
        public static extern int XMapWindow(IntPtr display, IntPtr window);
        [DllImport("libX11.so.6")]
        public static extern int XSetInputFocus(IntPtr display, IntPtr window, int revert_to, long time);
        [DllImport("libX11.so.6")]
        public static extern int XFlush(IntPtr display);
        public const int RevertToParent = 1;
        public const long CurrentTime = 0;
    }
}
