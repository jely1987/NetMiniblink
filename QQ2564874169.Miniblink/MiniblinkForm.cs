using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QQ2564874169.Miniblink
{
    public partial class MiniblinkForm : Form, IMessageFilter
    {
        /// <summary>
        /// 是否透明模式
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsTransparent { get; }
        /// <summary>
        /// 允许使用类样式控制窗体拖拽
        /// </summary>
        public bool DropByClass { get; set; }
        /// <summary>
        /// 是否允许在无边框模式下调整窗体大小
        /// </summary>
        public bool NoneBorderResize { get; set; }
        /// <summary>
        /// 窗体阴影长度
        /// </summary>
		public FormShadowWidth ShadowWidth { get; private set; }
        /// <summary>
        /// 调整大小的触发范围
        /// </summary>
        public FormResizeWidth ResizeWidth { get; }

        private FormWindowState _windowState = FormWindowState.Normal;
        private Rectangle? _stateRect;
        public new FormWindowState WindowState
        {
            get
            {
                return FormBorderStyle != FormBorderStyle.None ? base.WindowState : _windowState;
            }
            set
            {
                if (FormBorderStyle != FormBorderStyle.None)
                {
                    base.WindowState = value;
                    return;
                }
                if (_stateRect.HasValue == false)
                {
                    _stateRect = new Rectangle(Location, Size);
                }
                var rect = _stateRect.Value;

                if (value == FormWindowState.Maximized)
                {
                    if (_windowState != FormWindowState.Maximized)
                    {
                        _stateRect = new Rectangle(Location, Size);
                        Location = new Point(0, 0);
                        Size = Screen.PrimaryScreen.WorkingArea.Size;
                        base.WindowState = FormWindowState.Normal;
                    }
                }
                else if (value == FormWindowState.Minimized)
                {
                    if (base.WindowState == FormWindowState.Normal)
                    {
                        _stateRect = new Rectangle(Location, Size);
                    }

                    base.WindowState = value;
                }
                else if (value == FormWindowState.Normal)
                {
                    Location = rect.Location;
                    Size = rect.Size;
                    base.WindowState = value;
                }
                _windowState = value;
            }
        }
        public MiniblinkBrowser View { get; }

        private ResizeDirect _direct;
        private bool _resizeing;
        private Point _resizeStart;
        private Point _resizePos;
        private Size _resizeSize;
        private bool _isdrop;
        private Point _dropPos;
        private Point _dropLoc;
        private bool _bakMouseEn;
        private bool _bakTouchEn;
        private string _dragfunc;
        private string _maxfunc;
        private string _minfunc;
        private string _closefunc;

        public MiniblinkForm() : this(false)
        {

        }

        public MiniblinkForm(bool isTransparent)
        {
            Application.AddMessageFilter(this);
            InitializeComponent();
            Controls.Add(View = new MiniblinkBrowser
            {
                Dock = DockStyle.Fill
            });

            IsTransparent = isTransparent;

            if (!IsDesignMode())
            {
                ResizeWidth = new FormResizeWidth(5);

                if (IsTransparent)
                {
                    NoneBorderResize = true;
                    FormBorderStyle = FormBorderStyle.None;
                    View.PaintUpdated += Miniblink_Paint;
                }

                DropByClass = FormBorderStyle == FormBorderStyle.None;
                var tmp = Guid.NewGuid().ToString().Replace("-", "");
                View.BindNetFunc(new NetFunc(_dragfunc = "drag" + tmp, DropStart));
                View.BindNetFunc(new NetFunc(_maxfunc = "max" + tmp, MaxFunc));
                View.BindNetFunc(new NetFunc(_minfunc = "min" + tmp, MinFunc));
                View.BindNetFunc(new NetFunc(_closefunc = "close" + tmp, CloseFunc));

                View.DocumentReady += RegisterJsEvent;
                View.RegisterNetFunc(this);
            }
        }

        private void SetTransparent()
        {
            var style = WinApi.GetWindowLong(Handle, (int)WinConst.GWL_EXSTYLE);
            if ((style & (int)WinConst.WS_EX_LAYERED) != (int)WinConst.WS_EX_LAYERED)
            {
                WinApi.SetWindowLong(Handle, (int)WinConst.GWL_EXSTYLE, style | (int)WinConst.WS_EX_LAYERED);
            }
            MBApi.wkeSetTransparent(View.MiniblinkHandle, true);
        }

        private void SetTransparentStartPos(object sender, EventArgs e)
        {
            switch (StartPosition)
            {
                case FormStartPosition.CenterScreen:
                    Left = Screen.PrimaryScreen.WorkingArea.Width / 2 - Width / 2;
                    Top = Screen.PrimaryScreen.WorkingArea.Height / 2 - Height / 2;
                    break;
                case FormStartPosition.CenterParent:
                    if (ParentForm != null)
                    {
                        Left = ParentForm.Left + ParentForm.Width / 2 - Width / 2;
                        Top = ParentForm.Top + ParentForm.Height / 2 - Height / 2;
                    }
                    break;
            }
        }

        private void DropEvent(bool isRemove)
        {
            if (isRemove)
            {
                View.MouseEnabled = _bakMouseEn;
                View.TouchEnabled = _bakTouchEn;
                View.MouseMove -= DropMove;
                View.MouseUp -= DropUp;
                View.MouseLeave -= DropLeave;
            }
            else
            {
                _bakMouseEn = View.MouseEnabled;
                _bakTouchEn = View.TouchEnabled;
                View.MouseEnabled = false;
                View.TouchEnabled = false;
                View.MouseMove += DropMove;
                View.MouseUp += DropUp;
                View.MouseLeave += DropLeave;
            }
        }

        private void DropLeave(object sender, EventArgs e)
        {
            DropUp(sender, null);
        }

        private void DropUp(object sender, MouseEventArgs e)
        {
            _isdrop = false;
            DropEvent(true);
            Cursor = Cursors.Default;
        }

        private void DropMove(object sender, MouseEventArgs e)
        {
            var nx = MousePosition.X - _dropPos.X;
            var ny = MousePosition.Y - _dropPos.Y;
            nx = _dropLoc.X + nx;
            ny = _dropLoc.Y + ny;
            Location = new Point(nx, ny);
            Cursor = Cursors.SizeAll;
        }

        private object DropStart(NetFuncContext context)
        {
            if (DropByClass && _isdrop == false &&
                WindowState != FormWindowState.Maximized &&
                MouseButtons == MouseButtons.Left)
            {
                _isdrop = true;
                _dropPos = MousePosition;
                _dropLoc = Location;
                Cursor = Cursors.SizeAll;
                DropEvent(false);
            }

            return null;
        }

        private void MiniblinkForm_Load(object sender, EventArgs e)
        {
            if (!IsDesignMode() && IsTransparent)
            {
                Shown += SetTransparentStartPos;
                SetTransparent();
                using (var image = View.DrawToBitmap())
                {
                    TransparentPaint(image, image.Width, image.Height);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (IsTransparent && m.Msg == (int)WinConst.WM_SYSCOMMAND)
            {
                //窗口还原消息
                if (Utils.Dword(m.WParam).ToInt32() == 61728)
                {
                    using (var image = View.DrawToBitmap())
                    {
                        TransparentPaint(image, image.Width, image.Height);
                    }
                }
            }

            if (m.Msg == (int)WinConst.WM_NCPAINT)
            {
                DrawShadow();
            }
        }

        private void DrawShadow()
        {
            if (ShadowWidth == null)
            {
                return;
            }

            if (ShadowWidth.Bottom + ShadowWidth.Left + ShadowWidth.Right + ShadowWidth.Top < 1)
            {
                return;
            }

            var v = 2;
            WinApi.DwmSetWindowAttribute(Handle, 2, ref v, 4);
            var margins = new MARGINS
            {
                top = ShadowWidth.Top,
                left = ShadowWidth.Left,
                right = ShadowWidth.Right,
                bottom = ShadowWidth.Bottom
            };
            WinApi.DwmExtendFrameIntoClientArea(Handle, ref margins);
        }

        private void Miniblink_Paint(object sender, PaintUpdatedEventArgs e)
        {
            if (!IsDisposed && !IsDesignMode() && IsTransparent)
            {
                TransparentPaint(e.Image, e.Image.Width, e.Image.Height);

                e.Cancel = true;
            }
        }

        private void TransparentPaint(Bitmap bitmap, int width, int height)
        {
            var oldBits = IntPtr.Zero;
            var hBitmap = IntPtr.Zero;
            var memDc = WinApi.CreateCompatibleDC(IntPtr.Zero);

            try
            {
                var dst = new WinPoint { x = Left, y = Top };
                var src = new WinPoint();
                var size = new WinSize(width, height);

                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBits = WinApi.SelectObject(memDc, hBitmap);

                var blend = new BlendFunction
                {
                    BlendOp = (byte)WinConst.AC_SRC_OVER,
                    SourceConstantAlpha = 255,
                    AlphaFormat = (byte)WinConst.AC_SRC_ALPHA
                };

                WinApi.UpdateLayeredWindow(Handle, IntPtr.Zero, ref dst, ref size, memDc, ref src, 0, ref blend, (int)WinConst.ULW_ALPHA);
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    WinApi.SelectObject(memDc, oldBits);
                    WinApi.DeleteObject(hBitmap);
                }
                WinApi.DeleteDC(memDc);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;

                if (CheckAero() == false)
                {
                    cp.ClassStyle |= (int)WinConst.CS_DROPSHADOW;
                }
                else if (ShadowWidth == null)
                {
                    ShadowWidth = new FormShadowWidth();
                }

                return cp;
            }
        }

        private static bool CheckAero()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                var enabled = 0;
                WinApi.DwmIsCompositionEnabled(ref enabled);
                return enabled == 1;
            }
            return false;
        }

        private void ResizeTask()
        {
            var last = _resizeStart;
            var waiter = new SpinWait();

            Task.Factory.StartNew(() =>
            {
                while (_resizeing)
                {
                    if (MouseButtons != MouseButtons.Left)
                    {
                        _resizeing = false;
                        Invoke(new Action(() => { Cursor = DefaultCursor; }));
                        break;
                    }

                    var curr = MousePosition;
                    if (curr.X != last.X || curr.Y != last.Y)
                    {
                        var xx = curr.X - _resizeStart.X;
                        var xy = curr.Y - _resizeStart.Y;
                        int nx = Left, ny = Top, nw = Width, nh = Height;

                        switch (_direct)
                        {
                            case ResizeDirect.Left:
                                nw = _resizeSize.Width - xx;
                                nx = _resizePos.X + xx;
                                break;
                            case ResizeDirect.Right:
                                nw = _resizeSize.Width + xx;
                                break;
                            case ResizeDirect.Top:
                                nh = _resizeSize.Height - xy;
                                ny = _resizePos.Y + xy;
                                break;
                            case ResizeDirect.Bottom:
                                nh = _resizeSize.Height + xy;
                                break;
                            case ResizeDirect.LeftTop:
                                nw = _resizeSize.Width - xx;
                                nx = _resizePos.X + xx;
                                nh = _resizeSize.Height - xy;
                                ny = _resizePos.Y + xy;
                                break;
                            case ResizeDirect.LeftBottom:
                                nw = _resizeSize.Width - xx;
                                nx = _resizePos.X + xx;
                                nh = _resizeSize.Height + xy;
                                break;
                            case ResizeDirect.RightTop:
                                nw = _resizeSize.Width + xx;
                                nh = _resizeSize.Height - xy;
                                ny = _resizePos.Y + xy;
                                break;
                            case ResizeDirect.RightBottom:
                                nw = _resizeSize.Width + xx;
                                nh = _resizeSize.Height + xy;
                                break;
                        }

                        Invoke(new Action(() =>
                        {
                            Size = new Size(nw, nh);
                            Location = new Point(nx, ny);
                        }));
                    }

                    last = curr;
                    waiter.SpinOnce();
                }
            });
        }

        private ResizeDirect ShowResizeCursor(Point point)
        {
            var rect = ClientRectangle;
            var direct = ResizeDirect.None;

            if (point.X <= ResizeWidth.Left)
            {
                if (point.Y <= ResizeWidth.Top)
                {
                    direct = ResizeDirect.LeftTop;
                }
                else if (point.Y + ResizeWidth.Right >= rect.Height)
                {
                    direct = ResizeDirect.LeftBottom;
                }
                else
                {
                    direct = ResizeDirect.Left;
                }
            }
            else if (point.Y <= ResizeWidth.Top)
            {
                if (point.X <= ResizeWidth.Left)
                {
                    direct = ResizeDirect.LeftTop;
                }
                else if (point.X + ResizeWidth.Right >= rect.Width)
                {
                    direct = ResizeDirect.RightTop;
                }
                else
                {
                    direct = ResizeDirect.Top;
                }
            }
            else if (point.X + ResizeWidth.Right >= rect.Width)
            {
                if (point.Y <= ResizeWidth.Top)
                {
                    direct = ResizeDirect.RightTop;
                }
                else if (point.Y + ResizeWidth.Bottom >= rect.Height)
                {
                    direct = ResizeDirect.RightBottom;
                }
                else
                {
                    direct = ResizeDirect.Right;
                }
            }
            else if (point.Y + ResizeWidth.Bottom >= rect.Height)
            {
                if (point.X <= ResizeWidth.Left)
                {
                    direct = ResizeDirect.LeftBottom;
                }
                else if (point.X + ResizeWidth.Right >= rect.Width)
                {
                    direct = ResizeDirect.RightBottom;
                }
                else
                {
                    direct = ResizeDirect.Bottom;
                }
            }
            else if (_isdrop == false)
            {
                if (Cursor != DefaultCursor)
                {
                    Cursor = DefaultCursor;
                }
            }

            switch (direct)
            {
                case ResizeDirect.Bottom:
                case ResizeDirect.Top:
                    Cursor = Cursors.SizeNS;
                    break;
                case ResizeDirect.Left:
                case ResizeDirect.Right:
                    Cursor = Cursors.SizeWE;
                    break;
                case ResizeDirect.LeftTop:
                case ResizeDirect.RightBottom:
                    Cursor = Cursors.SizeNWSE;
                    break;
                case ResizeDirect.RightTop:
                case ResizeDirect.LeftBottom:
                    Cursor = Cursors.SizeNESW;
                    break;
            }

            return direct;
        }

        private object MaxFunc(NetFuncContext context)
        {
            WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            return null;
        }

        private object MinFunc(NetFuncContext context)
        {
            WindowState = FormWindowState.Minimized;
            return null;
        }

        private object CloseFunc(NetFuncContext context)
        {
            Close();
            return null;
        }

        private void RegisterJsEvent(object sender, DocumentReadyEventArgs e)
        {
            var map = new Dictionary<string, string>
            {
                {"maxName", _maxfunc},
                {"minName", _minfunc},
                {"closeName", _closefunc},
                {"dragName", _dragfunc}
            };
            var vars = string.Join(";", map.Keys.Select(k => $"var {k}='{map[k]}';")) + ";";
            var js = string.Join(".", typeof(MiniblinkForm).Namespace, "Files", "form.js");

            using (var sm = typeof(MiniblinkForm).Assembly.GetManifestResourceStream(js))
            {
                if (sm != null)
                {
                    using (var reader = new StreamReader(sm, Encoding.UTF8))
                    {
                        js = vars + reader.ReadToEnd();
                    }
                }
            }

            e.Frame.RunJs(js);
        }

        internal bool IsDesignMode()
        {
            return View.IsDesignMode();
        }

        private enum ResizeDirect
        {
            None,
            Left,
            Right,
            Top,
            Bottom,
            LeftTop,
            LeftBottom,
            RightTop,
            RightBottom
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (IsDisposed)
            {
                Application.RemoveMessageFilter(this);
                return false;
            }

            var ctrl = FromChildHandle(m.HWnd);

            if (ctrl == null || ctrl.FindForm() != this)
            {
                return false;
            }

            //鼠标单击
            if (m.Msg == (int)WinConst.WM_LBUTTONDOWN && _direct != ResizeDirect.None)
            {
                _resizeing = true;
                _resizeStart = MousePosition;
                _resizePos = Location;
                _resizeSize = Size;
                ResizeTask();
                return true;
            }

            //鼠标移动
            if (m.Msg == (int)WinConst.WM_MOUSEMOVE)
            {
                if (_resizeing)
                {
                    switch (_direct)
                    {
                        case ResizeDirect.Bottom:
                        case ResizeDirect.Top:
                            Cursor = Cursors.SizeNS;
                            break;
                        case ResizeDirect.Left:
                        case ResizeDirect.Right:
                            Cursor = Cursors.SizeWE;
                            break;
                        case ResizeDirect.LeftTop:
                        case ResizeDirect.RightBottom:
                            Cursor = Cursors.SizeNWSE;
                            break;
                        case ResizeDirect.RightTop:
                        case ResizeDirect.LeftBottom:
                            Cursor = Cursors.SizeNESW;
                            break;
                    }
                    return true;
                }
                if (NoneBorderResize && FormBorderStyle == FormBorderStyle.None &&
                    WindowState == FormWindowState.Normal)
                {
                    _direct = ShowResizeCursor(PointToClient(MousePosition));
                    return _direct != ResizeDirect.None;
                }
            }

            return false;
        }
    }
}

