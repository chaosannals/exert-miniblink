using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Drawing;
using System.IO;
using ExertMiniBlink.Core;
using System.Windows.Input;

namespace ExertMiniBlink.Wpf
{
    public class MiniBlinkBrowser : FrameworkElement
    {
        private IntPtr webView = IntPtr.Zero;
        private string url = string.Empty;
        private IntPtr buffer = IntPtr.Zero;
        private int bufferSize = 0;
        private MiniBlink.UrlChangedCallback2 onUrlChanged;
        private MiniBlink.wkePaintUpdatedCallback onPaintUpdated;

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public string Url
        {
            get { return url; }
            set
            {
                url = value;
                if (webView != IntPtr.Zero)
                {
                    MiniBlink.wkeLoadURLW(webView, value);
                }
            }
        }

        public MiniBlinkBrowser()
        {
            Loaded += (s, e) =>
            {
                Focusable = true;
                Focus();
                MiniBlink.Init();
                MiniBlink.wkeInitialize();
                webView = MiniBlink.wkeCreateWebView();
                IntPtr handle = (PresentationSource.FromVisual(this) as HwndSource).Handle;
                MiniBlink.wkeSetHandle(webView, handle);
                MiniBlink.wkeResize(webView, (int)ActualWidth, (int)ActualHeight);
                onUrlChanged = (webView, param, frameId, url) =>
                {
                    this.url = MiniBlink.wkeGetString(url).Utf8IntptrToString();

                };
                MiniBlink.wkeOnURLChanged2(webView, onUrlChanged, IntPtr.Zero);

                onPaintUpdated = (webview, param, hdc, x, y, cx, cy) =>
                {
                    InvalidateVisual();
                };
                MiniBlink.wkeOnPaintUpdated(webView, onPaintUpdated, IntPtr.Zero);

                Url = "http://www.baidu.com";
            };
            Unloaded += (s, e) =>
            {
                if (webView != IntPtr.Zero)
                {
                    MiniBlink.wkeDestroyWebView(webView);
                    webView = IntPtr.Zero;
                }
                if (buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(buffer);
                    buffer = IntPtr.Zero;
                }
            };

            
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (webView != IntPtr.Zero)
            {
                // 初始化缓冲区，屏幕大小改变是调整缓冲区大小。
                int size = (int)(ActualWidth * ActualHeight * 4);
                // Console.WriteLine(size);
                if (buffer == IntPtr.Zero || bufferSize != size)
                {
                    MiniBlink.wkeResize(webView, (int)ActualWidth, (int)ActualHeight);
                    Console.WriteLine($"render: {ActualWidth} * {ActualHeight}");
                    if (buffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(buffer);
                    }
                    bufferSize = size;
                    buffer = Marshal.AllocHGlobal(size);
                }

                // 绘图
                MiniBlink.wkePaint(webView, buffer, 0);
                using (Bitmap bmp = new Bitmap((int)ActualWidth, (int)ActualHeight, (int)(ActualWidth * 4), System.Drawing.Imaging.PixelFormat.Format32bppPArgb, buffer))
                {
                    IntPtr hbm = bmp.GetHbitmap();
                    ImageSource img = Imaging.CreateBitmapSourceFromHBitmap(hbm, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    drawingContext.DrawImage(img, new Rect(0, 0, ActualWidth, ActualHeight));
                    DeleteObject(hbm);
                }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (webView != IntPtr.Zero)
            {
                uint flags = GetMouseFlags(e);
                var p = e.GetPosition(this);
                MiniBlink.wkeFireMouseWheelEvent(webView, (int)p.X, (int)p.Y, e.Delta, flags);
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (webView != IntPtr.Zero)
            {
                uint msg = 0;
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        msg |= (uint)MiniBlink.wkeMouseMessage.WKE_MSG_LBUTTONDOWN;
                        break;
                    case MouseButton.Right:
                        msg |= (uint)MiniBlink.wkeMouseMessage.WKE_MSG_RBUTTONDOWN;
                        break;
                    case MouseButton.Middle:
                        msg |= (uint)MiniBlink.wkeMouseMessage.WKE_MSG_MBUTTONDOWN;
                        break;
                }
                uint flags = GetMouseFlags(e);
                var p = e.GetPosition(this);
                MiniBlink.wkeFireMouseEvent(webView, msg, (int)p.X, (int)p.Y, flags);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (webView != IntPtr.Zero)
            {
                uint msg = 0;
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        msg |= (uint)MiniBlink.wkeMouseMessage.WKE_MSG_LBUTTONUP;
                        break;
                    case MouseButton.Right:
                        msg |= (uint)MiniBlink.wkeMouseMessage.WKE_MSG_RBUTTONUP;
                        break;
                    case MouseButton.Middle:
                        msg |= (uint)MiniBlink.wkeMouseMessage.WKE_MSG_MBUTTONUP;
                        break;
                }
                var p = e.GetPosition(this);
                uint flags = GetMouseFlags(e);
                MiniBlink.wkeFireMouseEvent(webView, msg, (int)p.X, (int)p.Y, flags);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (webView != IntPtr.Zero)
            {
                uint flags = GetMouseFlags(e);
                var p = e.GetPosition(this);
                MiniBlink.wkeFireMouseEvent(webView, (uint)MiniBlinkMouseMessage.MOUSE_MOVE, (int)p.X, (int)p.Y, flags);
                switch (MiniBlink.wkeGetCursorInfoType(webView))
                {
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoPointer:
                        Cursor = Cursors.Arrow;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoCross:
                        Cursor = Cursors.Cross;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoIBeam:
                        Cursor = Cursors.IBeam;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoWait:
                        Cursor = Cursors.Wait;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoHelp:
                        Cursor = Cursors.Help;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoEastResize:
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoWestResize:
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoEastWestResize:
                        Cursor = Cursors.SizeWE;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoNorthResize:
                        Cursor = Cursors.SizeNS;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoNorthEastResize:
                        Cursor = Cursors.SizeNESW;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoNorthWestResize:
                        Cursor = Cursors.SizeNWSE;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoSouthResize:
                        Cursor = Cursors.SizeNS;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoSouthEastResize:
                        Cursor = Cursors.SizeNWSE;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoSouthWestResize:
                        Cursor = Cursors.SizeNESW;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoNorthSouthResize:
                        Cursor = Cursors.SizeNS;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoNorthEastSouthWestResize:
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoNorthWestSouthEastResize:
                        Cursor = Cursors.SizeAll;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoColumnResize:
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoRowResize:
                        Cursor = Cursors.Arrow;
                        break;
                    default:
                        Cursor = Cursors.Arrow;
                        break;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (webView != IntPtr.Zero)
            {
                Console.WriteLine($"kd: {(uint)e.Key}");
                MiniBlink.wkeFireKeyDownEvent(webView, (uint)e.Key, 0, false);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (webView != IntPtr.Zero)
            {
                Console.WriteLine($"ku: {(uint)e.Key}");
                MiniBlink.wkeFireKeyUpEvent(webView, (uint)e.Key, 0, false);
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            Console.WriteLine($"got focus");
            if (webView != IntPtr.Zero)
            {
                MiniBlink.wkeSetFocus(webView);
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            Console.WriteLine($"lost focus");
            if (webView != IntPtr.Zero)
            {
                MiniBlink.wkeKillFocus(webView);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            /*
            switch(e.Key)
            {
                case Key.Down:
                case Key.Up:
                case Key.Left:
                case Key.Right:
                case Key.Tab:
                    break;
            }
            */
        }

        private static uint GetMouseFlags(MouseEventArgs e)
        {
            uint flags = 0;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                flags |= (uint)MiniBlink.wkeMouseFlags.WKE_LBUTTON;
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                flags |= (uint)MiniBlink.wkeMouseFlags.WKE_MBUTTON;
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                flags |= (uint)MiniBlink.wkeMouseFlags.WKE_RBUTTON;
            }
            return flags;
        }
    }
}
