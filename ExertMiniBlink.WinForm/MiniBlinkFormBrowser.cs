using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using ExertMiniBlink.Core;

namespace ExertMiniBlink.WinForm
{
    public class MiniBlinkFormBrowser : Control
    {
        private IntPtr webView = IntPtr.Zero;
        private string url = string.Empty;
        private IntPtr buffer = IntPtr.Zero;
        private Size oldSize = Size.Empty;
        private MiniBlink.UrlChangedCallback2 onUrlChanged;
        private MiniBlink.wkePaintUpdatedCallback onPaintUpdated;

        public event EventHandler UrlChanged;

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

        public MiniBlinkFormBrowser()
        {
            ControlStyles style = ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.ResizeRedraw
                | ControlStyles.UserPaint;
            SetStyle(style, true);
            if (!DesignMode)
            {
                MiniBlink.Init();
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (!DesignMode)
            {
                MiniBlink.wkeInitialize();
                webView = MiniBlink.wkeCreateWebView();
                MiniBlink.wkeSetHandle(webView, Handle);
                MiniBlink.wkeResize(webView, Width, Height);

                onUrlChanged = (webView, param, frameId, url) =>
                {
                    this.url = MiniBlink.wkeGetString(url).Utf8IntptrToString();
                    if (UrlChanged != null)
                    {
                        UrlChanged(this, EventArgs.Empty);
                    }
                };
                MiniBlink.wkeOnURLChanged2(webView, onUrlChanged, IntPtr.Zero);

                onPaintUpdated = (webview, param, hdc, x, y, cx, cy) =>
                {
                    Invalidate();
                };
                MiniBlink.wkeOnPaintUpdated(webView, onPaintUpdated, IntPtr.Zero);


                Url = "http://baidu.com";
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (webView != IntPtr.Zero)
            {
                if (buffer == IntPtr.Zero || oldSize != Size)
                {
                    if (buffer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(buffer);
                    }
                    oldSize = Size;
                    buffer = Marshal.AllocHGlobal(Width * Height * 4);
                }

                MiniBlink.wkePaint(webView, buffer, 0);
                using (Bitmap bmp = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, buffer))
                {
                    e.Graphics.DrawImage(bmp, 0, 0);
                }
            }
            base.OnPaint(e);
            if (DesignMode)
            {
                e.Graphics.DrawString("MiniBlinkFormBrowser", this.Font, Brushes.Red, new Point());
                e.Graphics.DrawRectangle(Pens.Black, new Rectangle(0, 0, Width - 1, Height - 1));
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (webView != IntPtr.Zero && Width > 1 && Height > 1)
            {
                MiniBlink.wkeResize(webView, Width, Height);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (webView != IntPtr.Zero)
            {
                uint flags = GetMouseFlags(e);
                MiniBlink.wkeFireMouseWheelEvent(webView, e.X, e.Y, e.Delta, flags);
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (webView != IntPtr.Zero)
            {
                uint msg = 0;
                if (e.Button == MouseButtons.Left)
                {
                    msg = (uint)MiniBlink.wkeMouseMessage.WKE_MSG_LBUTTONDOWN;
                }
                else if (e.Button == MouseButtons.Middle)
                {
                    msg = (uint)MiniBlink.wkeMouseMessage.WKE_MSG_MBUTTONDOWN;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    msg = (uint)MiniBlink.wkeMouseMessage.WKE_MSG_RBUTTONDOWN;
                }
                uint flags = GetMouseFlags(e);
                MiniBlink.wkeFireMouseEvent(webView, msg, e.X, e.Y, flags);
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (webView != IntPtr.Zero)
            {
                uint msg = 0;
                if (e.Button == MouseButtons.Left)
                {
                    msg = (uint)MiniBlink.wkeMouseMessage.WKE_MSG_LBUTTONUP;
                }
                else if (e.Button == MouseButtons.Middle)
                {
                    msg = (uint)MiniBlink.wkeMouseMessage.WKE_MSG_MBUTTONUP;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    msg = (uint)MiniBlink.wkeMouseMessage.WKE_MSG_RBUTTONUP;
                }
                uint flags = GetMouseFlags(e);
                MiniBlink.wkeFireMouseEvent(webView, msg, e.X, e.Y, flags);
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (this.webView != IntPtr.Zero)
            {
                uint flags = GetMouseFlags(e);
                MiniBlink.wkeFireMouseEvent(webView, (uint)MiniBlinkMouseMessage.MOUSE_MOVE, e.X, e.Y, flags);

                switch (MiniBlink.wkeGetCursorInfoType(webView))
                {
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoPointer:
                        Cursor = Cursors.Default;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoCross:
                        Cursor = Cursors.Cross;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoHand:
                        Cursor = Cursors.Hand;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoIBeam:
                        Cursor = Cursors.IBeam;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoWait:
                        Cursor = Cursors.WaitCursor;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoHelp:
                        Cursor = Cursors.Help;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoEastResize:
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
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoWestResize:
                        Cursor = Cursors.SizeWE;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoNorthSouthResize:
                        Cursor = Cursors.SizeNS;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoEastWestResize:
                        Cursor = Cursors.SizeWE;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoNorthEastSouthWestResize:
                        Cursor = Cursors.SizeAll;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoNorthWestSouthEastResize:
                        Cursor = Cursors.SizeAll;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoColumnResize:
                        Cursor = Cursors.Default;
                        break;
                    case MiniBlink.WkeCursorInfo.WkeCursorInfoRowResize:
                        Cursor = Cursors.Default;
                        break;
                    default:
                        Cursor = Cursors.Default;
                        break;
                }
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (webView != IntPtr.Zero)
            {
                Console.WriteLine($"kd: {(uint)e.KeyValue}");
                MiniBlink.wkeFireKeyDownEvent(webView, (uint)e.KeyValue, 0, false);
            }
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (webView != IntPtr.Zero)
            {
                e.Handled = true;
                Console.WriteLine($"kp: {(uint)e.KeyChar}");
                MiniBlink.wkeFireKeyPressEvent(webView, (uint)e.KeyChar, 0, false);
            }
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (webView != IntPtr.Zero)
            {
                Console.WriteLine($"ku: {(uint)e.KeyValue}");
                MiniBlink.wkeFireKeyUpEvent(webView, (uint)e.KeyValue, 0, false);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            if (webView != IntPtr.Zero)
            {
                MiniBlink.wkeSetFocus(webView);
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (webView != IntPtr.Zero)
            {
                MiniBlink.wkeKillFocus(webView);
            }
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Down:
                case Keys.Up:
                case Keys.Left:
                case Keys.Right:
                case Keys.Tab:
                    e.IsInputKey = true;
                    break;
            }
        }
        private static uint GetMouseFlags(MouseEventArgs e)
        {
            uint flags = 0;
            if (e.Button == MouseButtons.Left)
            {
                flags = flags | (uint)MiniBlink.wkeMouseFlags.WKE_LBUTTON;
            }
            if (e.Button == MouseButtons.Middle)
            {
                flags = flags | (uint)MiniBlink.wkeMouseFlags.WKE_MBUTTON;
            }
            if (e.Button == MouseButtons.Right)
            {
                flags = flags | (uint)MiniBlink.wkeMouseFlags.WKE_RBUTTON;
            }
            if (Control.ModifierKeys == Keys.Control)
            {
                flags = flags | (uint)MiniBlink.wkeMouseFlags.WKE_CONTROL;
            }
            if (Control.ModifierKeys == Keys.Shift)
            {
                flags = flags | (uint)MiniBlink.wkeMouseFlags.WKE_SHIFT;
            }
            return flags;
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (webView != IntPtr.Zero)
            {//资源释放
                MiniBlink.wkeDestroyWebView(webView);
                webView = IntPtr.Zero;
            }
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
                buffer = IntPtr.Zero;
            }
        }
    }
}
