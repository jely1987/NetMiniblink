using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QQ2564874169.Miniblink
{
	public partial class MiniblinkForm : Form, IMiniblink
	{
        /// <summary>
        /// 允许使用类样式控制窗体拖拽
        /// </summary>
        public bool DropByClass { get; set; }

		private bool _noneBorderResize;
		/// <summary>
		/// 无边框模式下调整窗体大小
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool NoneBorderResize
		{
			get { return _noneBorderResize; }
			set
			{
				_noneBorderResize = value;

				if (_noneBorderResize)
				{
					_browser.WndMsg += ResizeMsg;
				}
				else
				{
					_browser.WndMsg -= ResizeMsg;
				}
			}
		}

		public FormShadowWidth ShadowWidth { get; }

		public override ContextMenuStrip ContextMenuStrip
		{
			get { return _browser.ContextMenuStrip; }
			set { _browser.ContextMenuStrip = value; }
		}

		private ResizeDirect _direct;
		private bool _resizeing;
		private Point _resize_start;
		private Point _resize_pos;
		private Size _resize_size;
		private bool _isdrop;
		private Point _dropstart;
		private Point _dropWinstart;
		private string _dragfunc;
		private string _maxfunc;
		private string _minfunc;
		private string _closefunc;
		private wkePaintUpdatedCallback _paintUpdated;
		private bool m_aeroEnabled;
		private const int CS_DROPSHADOW = 0x00020000;

		public MiniblinkForm(): this(false)
		{

		}

		public MiniblinkForm(bool isTransparent)
		{
			InitializeComponent();
            NoneBorderResize = true;

			IsTransparent = isTransparent;
			ShadowWidth = new FormShadowWidth();

            if (!Utils.IsDesignMode())
            {
                if (IsTransparent)
                {
                    FormBorderStyle = FormBorderStyle.None;
                    var style = WinApi.GetWindowLong(Handle, (int)WinConst.GWL_EXSTYLE);
                    if ((style & (int)WinConst.WS_EX_LAYERED) != (int) WinConst.WS_EX_LAYERED)
                    {
                        WinApi.SetWindowLong(Handle, (int)WinConst.GWL_EXSTYLE, style | (int)WinConst.WS_EX_LAYERED);
                    }
                    MBApi.wkeSetTransparent(_browser.MiniblinkHandle, true);
                    _browser.PaintUpdated += Miniblink_Paint;
                }

                DropByClass = true;
                var tmp = Guid.NewGuid().ToString().Replace("-", "");
                BindNetFunc(new NetFunc(_dragfunc = "drag" + tmp, DropFunc));
                BindNetFunc(new NetFunc(_maxfunc = "max" + tmp, MaxFunc));
                BindNetFunc(new NetFunc(_minfunc = "min" + tmp, MinFunc));
                BindNetFunc(new NetFunc(_closefunc = "close" + tmp, CloseFunc));

                _browser.WndMsg += DropWndMsg;
                _browser.WndMsg += ResizeMsg;
                DocumentReady += RegisterJsEvent;
                RegisterNetFunc(this);

                if (CheckAero())
                {
                    _browser.WndMsg += ShadowWndMsg;
                }
            }
		}

	    protected override void WndProc(ref Message m)
	    {
	        base.WndProc(ref m);

	        if (IsTransparent && (WinConst) m.Msg == WinConst.WM_SYSCOMMAND)
	        {
                //窗口还原消息
                if (Utils.Dword(m.WParam).ToInt32() == 61728)
                {
                    TransparentPaint(MBApi.wkeGetViewDC(MiniblinkHandle));
                }
	        }
        }

	    private void Miniblink_Paint(object sender, PaintUpdatedEventArgs e)
	    {
	        if (!IsDisposed && IsTransparent)
	        {
	            TransparentPaint(e.Hdc);
	            e.Cancel = true;
	        }
	    }

	    private void TransparentPaint(IntPtr mbHdc)
	    {
	        var point = new WinPoint();
	        var size = new WinSize(Width, Height);
	        var blend = new BlendFunction
	        {
	            BlendOp = (byte)WinConst.AC_SRC_OVER,
	            SourceConstantAlpha = 255,
	            AlphaFormat = (byte)WinConst.AC_SRC_ALPHA
	        };
	        WinApi.UpdateLayeredWindow(Handle, IntPtr.Zero, IntPtr.Zero, ref size, mbHdc, ref point, 0, ref blend, (int)WinConst.ULW_ALPHA);
	    }

        protected override CreateParams CreateParams
		{
			get
			{
				m_aeroEnabled = CheckAero();

				var cp = base.CreateParams;
				if (!m_aeroEnabled)
					cp.ClassStyle |= CS_DROPSHADOW;

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

        private void ShadowWndMsg(object sender, WndMsgEventArgs e)
        {
            if (e.Message != (int) WinConst.WM_NCPAINT)
                return;
            if (ShadowWidth.Bottom + ShadowWidth.Left + ShadowWidth.Right + ShadowWidth.Top < 1)
                return;
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

        private void DropWndMsg(object sender, WndMsgEventArgs e)
        {
            if (_isdrop == false)
            {
                return;
            }

            var lParam = Utils.Dword(e.LParam);
            var wMsg = (WinConst) e.Message;

            switch (wMsg)
            {
                case WinConst.WM_LBUTTONUP:
                    _isdrop = false;
                    Cursor = Cursors.Default;
                    break;
                case WinConst.WM_MOUSEMOVE:
                    var x = Utils.LOWORD(lParam);
                    var y = Utils.HIWORD(lParam);
                    var sp = PointToScreen(new Point(x, y));
                    var nx = sp.X - _dropstart.X;
                    var ny = sp.Y - _dropstart.Y;
                    nx = _dropWinstart.X + nx;
                    ny = _dropWinstart.Y + ny;
                    Location = new Point(nx, ny);
                    Cursor = Cursors.SizeAll;
                    e.Result = IntPtr.Zero;
                    break;
            }
        }

        private void ResizeTask()
		{
			var last = _resize_start;
			var waiter = new SpinWait();

			Task.Factory.StartNew(() =>
			{
				while (_resizeing)
				{
					if (MouseButtons != MouseButtons.Left)
						break;
                    
					var curr = MousePosition;
					if (curr.X != last.X || curr.Y != last.Y)
					{
						var xx = curr.X - _resize_start.X;
						var xy = curr.Y - _resize_start.Y;
						int nx = Left, ny = Top, nw = Width, nh = Height;

						switch (_direct)
						{
							case ResizeDirect.Left:
								nw = _resize_size.Width - xx;
								nx = _resize_pos.X + xx;
								break;
							case ResizeDirect.Right:
								nw = _resize_size.Width + xx;
								break;
							case ResizeDirect.Top:
								nh = _resize_size.Height - xy;
								ny = _resize_pos.Y + xy;
								break;
							case ResizeDirect.Bottom:
								nh = _resize_size.Height + xy;
								break;
							case ResizeDirect.LeftTop:
								nw = _resize_size.Width - xx;
								nx = _resize_pos.X + xx;
								nh = _resize_size.Height - xy;
								ny = _resize_pos.Y + xy;
								break;
							case ResizeDirect.LeftBottom:
								nw = _resize_size.Width - xx;
								nx = _resize_pos.X + xx;
								nh = _resize_size.Height + xy;
								break;
							case ResizeDirect.RightTop:
								nw = _resize_size.Width + xx;
								nh = _resize_size.Height - xy;
								ny = _resize_pos.Y + xy;
								break;
							case ResizeDirect.RightBottom:
								nw = _resize_size.Width + xx;
								nh = _resize_size.Height + xy;
								break;
						}
						this.UIInvoke(() =>
						{
							Size = new Size(nw, nh);
							Location = new Point(nx, ny);
						});
					}
					last = curr;
					waiter.SpinOnce();
				}
			});
		}

		private void ResizeMsg(object sender, WndMsgEventArgs e)
		{
            if (FormBorderStyle != FormBorderStyle.None)
            {
                return;
            }

            var lParam = e.LParam;
            var wMsg = (WinConst)e.Message;

			switch (wMsg)
			{
				case WinConst.WM_LBUTTONDOWN:
					if (_resizeing == false && _direct != ResizeDirect.None)
					{
						_resizeing = true;
						_resize_start = MousePosition;
						_resize_pos = Location;
						_resize_size = Size;
						ResizeTask();
                        e.Result = IntPtr.Zero;
					}
                    break;
				case WinConst.WM_LBUTTONUP:
					if (_resizeing)
					{
						_resizeing = false;
					}
                    break;
				case WinConst.WM_MOUSEMOVE:
					if(_resizeing == false)
					{
						const int p = 5;
						var x = Utils.LOWORD(lParam);
						var y = Utils.HIWORD(lParam);
						var rect = ClientRectangle;
						var direct = ResizeDirect.None;

						if (x <= p && x > 0)
						{
							if (y <= p && y > 0)
							{
								direct = ResizeDirect.LeftTop;
							}
							else if (y >= rect.Height - p && y < rect.Height)
							{
								direct = ResizeDirect.LeftBottom;
							}
							else
							{
								direct = ResizeDirect.Left;
							}
						}
						else if (y <= p && y > 0)
						{
							if (x <= p && x > 0)
							{
								direct = ResizeDirect.LeftTop;
							}
							else if (x >= rect.Width - p && x < rect.Width)
							{
								direct = ResizeDirect.RightTop;
							}
							else
							{
								direct = ResizeDirect.Top;
							}
						}
						else if (x >= rect.Width - p && x < rect.Width)
						{
							if (y <= p && y > 0)
							{
								direct = ResizeDirect.RightTop;
							}
							else if (y >= rect.Height - p && y < rect.Height)
							{
								direct = ResizeDirect.RightBottom;
							}
							else
							{
								direct = ResizeDirect.Right;
							}
						}
						else if (y >= rect.Height - p && y < rect.Height)
						{
							if (x <= p && x > 0)
							{
								direct = ResizeDirect.LeftBottom;
							}
							else if (x >= rect.Width - p && x < rect.Width)
							{
								direct = ResizeDirect.RightBottom;
							}
							else
							{
								direct = ResizeDirect.Bottom;
							}
						}
						else if (_direct != ResizeDirect.None)
						{
							Cursor = Cursors.Default;
							_direct = ResizeDirect.None;
							return;
						}

						_direct = direct;
                    }
                    else
                    {
                        e.Result = IntPtr.Zero;
                    }

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
                    break;
			}
		}

		private object DropFunc(NetFuncContext context)
		{
			if (_isdrop == false && WindowState != FormWindowState.Maximized && MouseButtons == MouseButtons.Left)
			{
				_isdrop = true;
				_dropstart = MousePosition;
				_dropWinstart = Location;
				Cursor = Cursors.SizeAll;
			}
			return null;
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
		    if (DropByClass == false) return;

			e.Frame.RunJs(@"
                document.getElementsByTagName('body')[0].addEventListener('mousedown',
                    function(e) {
                        var obj = e.target || e.srcElement;
                        if ({ 'INPUT': 1, 'SELECT': 1,'IMG': 1 }[obj.tagName.toUpperCase()])
                        return;

                        while (obj) {
                            for (var i = 0; i<obj.classList.length; i++) {
                                if (obj.classList[i] === 'mbform-nodrag')
                                    return;
                                if (obj.classList[i] === 'mbform-drag') {
                                    " + _dragfunc + @"(e.screenX, e.screenY);
                                    return;
                                }
                            }
                            obj = obj.parentElement;
                        }
                    });


                var els = document.getElementsByClassName('mbform-max');
                for (var i=0;i<els.length;i++)
                {
                    els[i].addEventListener('click',
                        function() {" + _maxfunc + @"(); });
                }

                els = document.getElementsByClassName('mbform-min');
                for (var i=0;i<els.length;i++)
                {
                    els[i].addEventListener('click',
                        function() {" + _minfunc + @"(); });
                }

                els = document.getElementsByClassName('mbform-close');
                for (var i=0;i<els.length;i++)
                {
                    els[i].addEventListener('click',
                        function() {" + _closefunc + @"(); });
                }
            ");
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsTransparent { get; set; }

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public float Zoom
		{
			get { return _browser.Zoom; }
			set { _browser.Zoom = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CookieEnabled
		{
			get { return _browser.CookieEnabled; }
			set { _browser.CookieEnabled = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string UserAgent
		{
			get { return _browser.UserAgent; }
			set { _browser.UserAgent = value; }
		}

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ScrollTop
        {
            get { return _browser.ScrollTop; }
            set { _browser.ScrollTop = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ScrollLeft
        {
            get { return _browser.ScrollLeft; }
            set { _browser.ScrollLeft = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ScrollHeight
        {
            get { return _browser.ScrollHeight; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ScrollWidth
        {
            get { return _browser.ScrollWidth; }
        }

        private FormWindowState _windowState = FormWindowState.Normal;
		private Rectangle? _stateRect;
		public new FormWindowState WindowState
		{
			get { return _windowState; }
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
				else if (value == FormWindowState.Normal)
				{
					Location = rect.Location;
					Size = rect.Size;
					base.WindowState = value;
				}
				else
				{
					base.WindowState = value;
				}
				_windowState = value;
			}
		}

        public IntPtr MiniblinkHandle
        {
            get { return _browser.MiniblinkHandle; }
        }
        public string LocalDomain => _browser.LocalDomain;
		public string LocalResourceDir => _browser.LocalResourceDir;
		public string Url => _browser.Url;
		public bool IsDocumentReady => _browser.IsDocumentReady;
		public string DocumentTitle => _browser.DocumentTitle;
		public int DocumentWidth => _browser.DocumentWidth;
		public int DocumentHeight => _browser.DocumentHeight;
		public int ContentWidth => _browser.ContentWidth;
		public int ContentHeight => _browser.ContentHeight;
		public bool CanGoBack => _browser.CanGoBack;
		public bool CanGoForward => _browser.CanGoForward;

        public MBDeviceParameter DeviceParameter
        {
            get { return _browser.DeviceParameter; }
        }

        public event EventHandler<UrlChangedEventArgs> UrlChanged
		{
			add { _browser.UrlChanged += value; }
			remove { _browser.UrlChanged -= value; }
		}
		public event EventHandler<NavigateEventArgs> NavigateBefore
		{
			add { _browser.NavigateBefore += value; }
			remove { _browser.NavigateBefore -= value; }
		}
		public event EventHandler<DocumentReadyEventArgs> DocumentReady
		{
			add { _browser.DocumentReady += value; }
			remove { _browser.DocumentReady -= value; }
		}
		public event EventHandler<ConsoleMessageEventArgs> ConsoleMessage
		{
			add { _browser.ConsoleMessage += value; }
			remove { _browser.ConsoleMessage -= value; }
		}
		public event EventHandler<NetResponseEventArgs> NetResponse
		{
			add { _browser.NetResponse += value; }
			remove { _browser.NetResponse -= value; }
		}
		public event EventHandler<LoadUrlBeginEventArgs> LoadUrlBegin
		{
			add { _browser.LoadUrlBegin += value; }
			remove { _browser.LoadUrlBegin -= value; }
		}
        public event EventHandler<DownloadEventArgs> Download
        {
            add { _browser.Download += value; }
            remove { _browser.Download -= value; }
        }

        public event EventHandler<AlertEventArgs> AlertBefore
        {
            add { _browser.AlertBefore += value; }
            remove { _browser.AlertBefore -= value; }
        }

        public event EventHandler<ConfirmEventArgs> ConfirmBefore
        {
            add { _browser.ConfirmBefore += value; }
            remove { _browser.ConfirmBefore -= value; }
        }
        public event EventHandler<PromptEventArgs> PromptBefore
        {
            add { _browser.PromptBefore += value; }
            remove { _browser.PromptBefore -= value; }
        }

        public event EventHandler<WndMsgEventArgs> WndMsg
	    {
	        add { _browser.WndMsg += value; }
	        remove { _browser.WndMsg -= value; }
	    }

	    public event EventHandler<PaintUpdatedEventArgs> PaintUpdated
	    {
	        add { _browser.PaintUpdated += value; }
	        remove { _browser.PaintUpdated -= value; }
	    }

	    public event EventHandler<DidCreateScriptContextEventArgs> DidCreateScriptContext
	    {
	        add { _browser.DidCreateScriptContext += value; }
	        remove { _browser.DidCreateScriptContext -= value; }
	    }

        public void ScrollTo(int x, int y)
        {
            _browser.ScrollTo(x, y);
        }

        public void RegisterNetFunc(object target)
		{
			_browser.RegisterNetFunc(target);
		}

		public void ShowDevTools()
		{
			_browser.ShowDevTools();
		}

		public object RunJs(string script)
		{
			return _browser.RunJs(script);
		}

		public object CallJsFunc(string funcName, params object[] param)
		{
			return _browser.CallJsFunc(funcName, param);
		}

		public void BindNetFunc(NetFunc func)
		{
			_browser.BindNetFunc(func);
		}

		public void SetLocalResource(string dir, string domain)
		{
			_browser.SetLocalResource(dir, domain);
		}

		public void SetHeadlessEnabled(bool enable)
		{
			_browser.SetHeadlessEnabled(enable);
		}

		public void SetNpapiPluginsEnable(bool enable)
		{
			_browser.SetNpapiPluginsEnable(enable);
		}

		public void SetNavigationToNewWindow(bool enable)
		{
			_browser.SetNavigationToNewWindow(enable);
		}

		public void SetCspCheckEnable(bool enable)
		{
			_browser.SetCspCheckEnable(enable);
		}

        public void SetTouchEnabled(bool enable)
        {
            _browser.SetTouchEnabled(enable);
        }

        public void SetMouseEnabled(bool enable)
        {
            _browser.SetMouseEnabled(enable);
        }

        public bool GoForward()
		{
			return _browser.GoForward();
		}

		public void EditorSelectAll()
		{
			_browser.EditorSelectAll();
		}

		public void EditorUnSelect()
		{
			_browser.EditorUnSelect();
		}

		public void EditorCopy()
		{
			_browser.EditorCopy();
		}

		public void EditorCut()
		{
			_browser.EditorCut();
		}

		public void EditorPaste()
		{
			_browser.EditorPaste();
		}

		public void EditorDelete()
		{
			_browser.EditorDelete();
		}

		public void EditorUndo()
		{
			_browser.EditorUndo();
		}

		public void EditorRedo()
		{
			_browser.EditorRedo();
		}

		public bool GoBack()
		{
			return _browser.GoBack();
		}

		public void SetProxy(WKEProxy proxy)
		{
			_browser.SetProxy(proxy);
		}

		public void LoadUri(string uri)
		{
			_browser.LoadUri(uri);
		}

		public void LoadHtml(string html, string baseUrl = null)
		{
			_browser.LoadHtml(html, baseUrl);
		}

		public void StopLoading()
		{
			_browser.StopLoading();
		}

		public void Reload()
		{
			_browser.Reload();
		}

        public void PrintToBitmap(Action<Image> callback)
        {
            _browser.PrintToBitmap(callback);
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

        private void MiniblinkForm_Load(object sender, EventArgs e)
        {
            if (!Utils.IsDesignMode() && IsTransparent)
            {
                TransparentPaint(MBApi.wkeGetViewDC(MiniblinkHandle));
            }
        }
    }
}

