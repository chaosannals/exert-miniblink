using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using ExertMiniBlink.Core.Properties;

namespace ExertMiniBlink.Core
{
    public static class MiniBlink
    {
        const string DllName = "node.dll";
        public static void Init()
        {
            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DllName);
            if (!File.Exists(dllPath))
            {
                File.WriteAllBytes(dllPath, Resources.node);
            }
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void wkeSetHandle(IntPtr webView, IntPtr wnd);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        public static extern IntPtr wkeGetStringW(IntPtr text);

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        public static extern IntPtr wkeToStringW(IntPtr text);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr wkeGetString(IntPtr text);

        [DllImport(DllName)]
        public static extern void wkeSetDebugConfig(IntPtr webView, string debugString, IntPtr param);

        [DllImport(DllName)]
        public static extern Int64 wkeRunJSW(IntPtr webView, [In][MarshalAs(UnmanagedType.LPWStr)] string script);

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool wkeFireMouseEvent(IntPtr webView, uint message, int x, int y, uint flags);

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool wkeFireMouseWheelEvent(IntPtr webView, int x, int y, int delta, uint flags);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool wkeFireKeyUpEvent(IntPtr webView, uint virtualKeyCode, uint flags, [MarshalAs(UnmanagedType.I1)] bool systemKey);

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool wkeFireKeyDownEvent(IntPtr webView, uint virtualKeyCode, uint flags, [MarshalAs(UnmanagedType.I1)] bool systemKey);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool wkeFireKeyPressEvent(IntPtr webView, uint charCode, uint flags, [MarshalAs(UnmanagedType.I1)] bool systemKey);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void wkeSetFocus(IntPtr webView);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void wkeKillFocus(IntPtr webView);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void wkeResize(IntPtr webView, int w, int h);

        [DllImport(DllName)]
        public static extern IntPtr wkeCreateWebView();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void wkeDestroyWebView(IntPtr webView);

        [DllImport(DllName)]
        public static extern void wkeInitialize();

        [DllImport(DllName)]
        public static extern void wkeLoadFile(IntPtr webView, [In, MarshalAs(UnmanagedType.LPWStr)] string filename);

        [DllImport(DllName)]
        public static extern void wkeLoadHTML(System.IntPtr webView, [In()][MarshalAs(UnmanagedType.LPStr)] string html);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void wkeOnURLChanged2(IntPtr webView, UrlChangedCallback2 callback, IntPtr callbackParam);

        [DllImport(DllName, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void wkeLoadURLW(IntPtr webView, [In, MarshalAs(UnmanagedType.LPWStr)] string url);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void wkeOnPaintUpdated(IntPtr webView, wkePaintUpdatedCallback callback, IntPtr callbackParam);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void wkePaint(IntPtr webView, IntPtr bits, int pitch);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern WkeCursorInfo wkeGetCursorInfoType(IntPtr webView);
        public static string Utf8IntptrToString(this IntPtr ptr)
        {
            var data = new List<byte>();
            var off = 0;
            while (true)
            {
                var ch = Marshal.ReadByte(ptr, off++);
                if (ch == 0)
                {
                    break;
                }
                data.Add(ch);
            }
            return Encoding.UTF8.GetString(data.ToArray());
        }

        //调用这个之后要手动调用  Marshal.FreeHGlobal(ptr);
        public static IntPtr Utf8StringToIntptr(this string str)
        {
            byte[] utf8bytes = Encoding.UTF8.GetBytes(str);
            IntPtr ptr = Marshal.AllocHGlobal(utf8bytes.Length + 1);
            Marshal.Copy(utf8bytes, 0, ptr, utf8bytes.Length);
            Marshal.WriteByte(ptr, utf8bytes.Length, 0);
            return ptr;
        }

        public enum WkeCursorInfo : uint
        {
            WkeCursorInfoPointer = 0,
            WkeCursorInfoCross = 1,
            WkeCursorInfoHand = 2,
            WkeCursorInfoIBeam = 3,
            WkeCursorInfoWait = 4,
            WkeCursorInfoHelp = 5,
            WkeCursorInfoEastResize = 6,
            WkeCursorInfoNorthResize = 7,
            WkeCursorInfoNorthEastResize = 8,
            WkeCursorInfoNorthWestResize = 9,
            WkeCursorInfoSouthResize = 10,
            WkeCursorInfoSouthEastResize = 11,
            WkeCursorInfoSouthWestResize = 12,
            WkeCursorInfoWestResize = 13,
            WkeCursorInfoNorthSouthResize = 14,
            WkeCursorInfoEastWestResize = 15,
            WkeCursorInfoNorthEastSouthWestResize = 16,
            WkeCursorInfoNorthWestSouthEastResize = 17,
            WkeCursorInfoColumnResize = 18,
            WkeCursorInfoRowResize = 19,
        }

        public enum wkeMouseMessage : uint
        {
            WKE_MSG_MOUSEMOVE = 0x0200,
            WKE_MSG_LBUTTONDOWN = 0x0201,
            WKE_MSG_LBUTTONUP = 0x0202,
            WKE_MSG_LBUTTONDBLCLK = 0x0203,
            WKE_MSG_RBUTTONDOWN = 0x0204,
            WKE_MSG_RBUTTONUP = 0x0205,
            WKE_MSG_RBUTTONDBLCLK = 0x0206,
            WKE_MSG_MBUTTONDOWN = 0x0207,
            WKE_MSG_MBUTTONUP = 0x0208,
            WKE_MSG_MBUTTONDBLCLK = 0x0209,
            WKE_MSG_MOUSEWHEEL = 0x020A,
        }
        public enum wkeMouseFlags : uint
        {
            WKE_LBUTTON = 0x01,
            WKE_RBUTTON = 0x02,
            WKE_SHIFT = 0x04,
            WKE_CONTROL = 0x08,
            WKE_MBUTTON = 0x10,
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void wkePaintUpdatedCallback(IntPtr webView, IntPtr param, IntPtr hdc, int x, int y, int cx, int cy);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void UrlChangedCallback2(IntPtr webView, IntPtr param, IntPtr frameId, IntPtr url);
    }
}
