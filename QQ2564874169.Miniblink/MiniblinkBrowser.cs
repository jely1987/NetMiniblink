using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QQ2564874169.Miniblink
{
    public partial class MiniblinkBrowser : UserControl, IMiniblink
    {
        #region 属性

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IntPtr MiniblinkHandle { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MBDeviceParameter DeviceParameter { get; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Url => MBApi.wkeGetURL(MiniblinkHandle).ToUTF8String() ?? string.Empty;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDocumentReady => MBApi.wkeIsDocumentReady(MiniblinkHandle);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DocumentTitle => MBApi.wkeGetTitle(MiniblinkHandle).ToUTF8String() ?? string.Empty;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int DocumentWidth => MBApi.wkeGetWidth(MiniblinkHandle);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int DocumentHeight => MBApi.wkeGetHeight(MiniblinkHandle);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ContentWidth => MBApi.wkeGetContentWidth(MiniblinkHandle);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ContentHeight => MBApi.wkeGetContentHeight(MiniblinkHandle);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ViewWidth => MBApi.wkeGetWidth(MiniblinkHandle);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ViewHeight => MBApi.wkeGetHeight(MiniblinkHandle);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanGoBack => MBApi.wkeCanGoBack(MiniblinkHandle);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CanGoForward => MBApi.wkeCanGoForward(MiniblinkHandle);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Zoom
        {
            get { return DesignMode ? 0 : MBApi.wkeGetZoomFactor(MiniblinkHandle); }
            set
            {
                if (!DesignMode)
                {
                    MBApi.wkeSetZoomFactor(MiniblinkHandle, value);
                }
            }
        }

        private bool _cookieEnabled = true;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CookieEnabled
        {
            get { return _cookieEnabled; }
            set
            {
                _cookieEnabled = value;

                if (!DesignMode)
                {
                    MBApi.wkeSetCookieEnabled(MiniblinkHandle, _cookieEnabled);

                    if (_cookieEnabled)
                    {
                        LoadUrlBegin -= ClearCookie;
                    }
                    else
                    {
                        LoadUrlBegin += ClearCookie;
                    }
                }
            }
        }

        private void ClearCookie(object sender, LoadUrlBeginEventArgs e)
        {
            if (_cookieEnabled) return;
            MBApi.wkePerformCookieCommand(MiniblinkHandle, wkeCookieCommand.ClearAllCookies);
            MBApi.wkePerformCookieCommand(MiniblinkHandle, wkeCookieCommand.ClearSessionCookies);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string UserAgent
        {
            get { return DesignMode ? "" : MBApi.wkeGetUserAgent(MiniblinkHandle).ToUTF8String(); }
            set
            {
                if (!DesignMode)
                {
                    MBApi.wkeSetUserAgent(MiniblinkHandle, value);
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ScrollTop
        {
            get
            {
                return DesignMode
                    ? 0
                    : Convert.ToInt32(
                        RunJs("return Math.max(document.documentElement.scrollTop,document.body.scrollTop)"));
            }
            set
            {
                if (!DesignMode)
                {
                    ScrollTo(ScrollLeft, value);
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ScrollLeft
        {
            get
            {
                return DesignMode
                    ? 0
                    : Convert.ToInt32(
                        RunJs("return Math.max(document.documentElement.scrollLeft,document.body.scrollLeft)"));
            }
            set
            {
                if (!DesignMode)
                {
                    ScrollTo(value, ScrollTop);
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ScrollHeight
        {
            get
            {
                return DesignMode
                    ? 0
                    : Convert.ToInt32(
                        RunJs("return Math.max(document.documentElement.scrollHeight,document.body.scrollHeight)"));
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ScrollWidth
        {
            get
            {
                return DesignMode
                    ? 0
                    : Convert.ToInt32(
                        RunJs("return Math.max(document.documentElement.scrollWidth,document.body.scrollWidth)"));
            }
        }

        #endregion

        #region 事件

        private wkeDidCreateScriptContextCallback _wkeDidCreateScriptContextCallback;
        private EventHandler<DidCreateScriptContextEventArgs> _didCreateScriptContextCallback;

        public event EventHandler<DidCreateScriptContextEventArgs> DidCreateScriptContext
        {
            add
            {
                if (_wkeDidCreateScriptContextCallback == null)
                {
                    _wkeDidCreateScriptContextCallback = new wkeDidCreateScriptContextCallback(onWkeDidCreateScriptContextCallback);
                    MBApi.wkeOnDidCreateScriptContext(MiniblinkHandle, _wkeDidCreateScriptContextCallback, IntPtr.Zero);
                }

                _didCreateScriptContextCallback += value;
            }
            remove { _didCreateScriptContextCallback -= value; }
        }

        protected virtual void onWkeDidCreateScriptContextCallback(IntPtr webView, IntPtr param, IntPtr frame,
            IntPtr context,
            int extensionGroup, int worldId)
        {
            var e = new DidCreateScriptContextEventArgs
            {
                Frame = new FrameContext(this, frame)
            };
            _didCreateScriptContextCallback?.Invoke(this, e);
        }

        private wkeURLChangedCallback2 _wkeUrlChanged;
        private EventHandler<UrlChangedEventArgs> _urlChanged;

        public event EventHandler<UrlChangedEventArgs> UrlChanged
        {
            add
            {
                if (_wkeUrlChanged == null)
                {
                    MBApi.wkeOnURLChanged2(MiniblinkHandle, _wkeUrlChanged = new wkeURLChangedCallback2(OnUrlChanged),
                        IntPtr.Zero);
                }

                _urlChanged += value;
            }
            remove { _urlChanged -= value; }
        }

        protected virtual void OnUrlChanged(IntPtr mb, IntPtr param, IntPtr frame, IntPtr url)
        {
            _urlChanged?.Invoke(this, new UrlChangedEventArgs
            {
                Url = url.WKEToUTF8String(),
                Frame = new FrameContext(this, frame)
            });
        }

        private wkeNavigationCallback _wkeNavigateBefore;
        private EventHandler<NavigateEventArgs> _navigateBefore;

        public event EventHandler<NavigateEventArgs> NavigateBefore
        {
            add
            {
                if (_wkeNavigateBefore == null)
                {
                    MBApi.wkeOnNavigation(MiniblinkHandle,
                        _wkeNavigateBefore = new wkeNavigationCallback(OnNavigateBefore),
                        IntPtr.Zero);
                }

                _navigateBefore += value;
            }
            remove
            {
                _navigateBefore -= value;

                if (_navigateBefore == null)
                {
                    MBApi.wkeOnNavigation(MiniblinkHandle, null, IntPtr.Zero);
                }
            }
        }

        protected virtual byte OnNavigateBefore(IntPtr mb, IntPtr param, wkeNavigationType type, IntPtr url)
        {
            if (_navigateBefore == null)
                return 1;

            var e = new NavigateEventArgs
            {
                Url = url.WKEToUTF8String(),
                Type = type
            };
            _navigateBefore(this, e);

            return (byte) (e.Cancel ? 0 : 1);
        }

        private wkeDocumentReady2Callback _wkeDocumentReady;
        private EventHandler<DocumentReadyEventArgs> _documentReady;

        public event EventHandler<DocumentReadyEventArgs> DocumentReady
        {
            add
            {
                if (_wkeDocumentReady == null)
                {
                    MBApi.wkeOnDocumentReady2(MiniblinkHandle,
                        _wkeDocumentReady = new wkeDocumentReady2Callback(OnDocumentReady),
                        IntPtr.Zero);
                }

                _documentReady += value;
            }
            remove { _documentReady -= value; }
        }

        protected virtual void OnDocumentReady(IntPtr mb, IntPtr param, IntPtr frameId)
        {
            _documentReady?.Invoke(this, new DocumentReadyEventArgs() {Frame = new FrameContext(this, frameId)});
        }

        private wkeConsoleCallback _wkeConsoleMessage;
        private EventHandler<ConsoleMessageEventArgs> _consoleMessage;

        public event EventHandler<ConsoleMessageEventArgs> ConsoleMessage
        {
            add
            {
                if (_wkeConsoleMessage == null)
                {
                    MBApi.wkeOnConsole(MiniblinkHandle, _wkeConsoleMessage = new wkeConsoleCallback(OnConsoleMessage),
                        IntPtr.Zero);
                }

                _consoleMessage += value;
            }
            remove
            {
                _consoleMessage -= value;

                if (_consoleMessage == null)
                {
                    MBApi.wkeOnConsole(MiniblinkHandle, null, IntPtr.Zero);
                }
            }
        }

        protected virtual void OnConsoleMessage(IntPtr mb, IntPtr param, wkeConsoleLevel level, IntPtr message,
            IntPtr sourceName, uint sourceLine, IntPtr stackTrace)
        {
            _consoleMessage?.Invoke(this, new ConsoleMessageEventArgs
            {
                Level = level,
                Message = message.WKEToUTF8String(),
                SourceLine = (int) sourceLine,
                SourceName = sourceName.WKEToUTF8String(),
                StackTrace = stackTrace.WKEToUTF8String()
            });
        }

        private wkeNetResponseCallback _wkeNetResponse;
        private EventHandler<NetResponseEventArgs> _netResponse;

        public event EventHandler<NetResponseEventArgs> NetResponse
        {
            add
            {
                if (_wkeNetResponse == null)
                {
                    MBApi.wkeNetOnResponse(MiniblinkHandle, _wkeNetResponse = new wkeNetResponseCallback(OnNetResponse),
                        IntPtr.Zero);
                }

                _netResponse += value;
            }
            remove
            {
                _netResponse -= value;

                if (_netResponse == null)
                {
                    MBApi.wkeNetOnResponse(MiniblinkHandle, null, IntPtr.Zero);
                }
            }
        }

        protected virtual bool OnNetResponse(IntPtr mb, IntPtr param, string url, IntPtr job)
        {
            if (_netResponse == null)
                return true;

            var e = new NetResponseEventArgs
            {
                Job = job,
                Url = url,
                ContentType = MBApi.wkeNetGetMIMEType(job).ToUTF8String()
            };
            _netResponse(this, e);

            if (e.Data != null)
            {
                NetSetData(e.Job, e.Data);
                return true;
            }

            return e.Cancel;
        }

        private wkeLoadUrlBeginCallback _wkeLoadUrlBegin;
        private wkeLoadUrlEndCallback _wkeLoadUrlEnd;
        private EventHandler<LoadUrlBeginEventArgs> _loadUrlBegin;

        public event EventHandler<LoadUrlBeginEventArgs> LoadUrlBegin
        {
            add
            {
                if (_wkeLoadUrlBegin == null)
                {
                    MBApi.wkeOnLoadUrlBegin(MiniblinkHandle,
                        _wkeLoadUrlBegin = new wkeLoadUrlBeginCallback(OnLoadUrlBegin),
                        IntPtr.Zero);
                    MBApi.wkeOnLoadUrlEnd(MiniblinkHandle, _wkeLoadUrlEnd = new wkeLoadUrlEndCallback(OnLoadUrlEnd),
                        IntPtr.Zero);
                }

                _loadUrlBegin += value;
            }
            remove { _loadUrlBegin -= value; }
        }

        private wkeDownloadCallback _wkeDownload;
        private EventHandler<DownloadEventArgs> _download;

        public event EventHandler<DownloadEventArgs> Download
        {
            add
            {
                if (_wkeDownload == null)
                {
                    MBApi.wkeOnDownload(MiniblinkHandle, _wkeDownload = new wkeDownloadCallback(OnDownload),
                        IntPtr.Zero);
                }

                _download += value;
            }
            remove { _download -= value; }
        }

        public event EventHandler<AlertEventArgs> AlertBefore;

        protected virtual void OnAlertBefore(AlertEventArgs e)
        {
            AlertBefore?.Invoke(this, e);
        }

        public event EventHandler<ConfirmEventArgs> ConfirmBefore;

        protected virtual void OnConfirmBefore(ConfirmEventArgs e)
        {
            ConfirmBefore?.Invoke(this, e);
        }

        public event EventHandler<PromptEventArgs> PromptBefore;

        protected virtual void OnPromptBefore(PromptEventArgs e)
        {
            PromptBefore?.Invoke(this, e);
        }

        protected virtual byte OnDownload(IntPtr mb, IntPtr param, IntPtr url)
        {
            var e = new DownloadEventArgs
            {
                Url = url.ToUTF8String()
            };
            _download?.Invoke(this, e);
            return 0;
        }

        public event EventHandler<PaintUpdatedEventArgs> PaintUpdated;

        protected virtual void OnPaintUpdated(IntPtr mb, IntPtr param, IntPtr hdc, int x, int y, int w, int h)
        {
            var e = new PaintUpdatedEventArgs
            {
                WebView = mb,
                Param = param,
                Hdc = hdc,
                X = x,
                Y = y,
                Width = w,
                Height = h
            };
            PaintUpdated?.Invoke(this, e);

            if (!e.Cancel)
            {
                _browserPaintUpdated(this, e);
            }
        }

        private void JobCompleted(NetJob job)
        {
            if (job.Data != null)
            {
                this.UIInvoke(() =>
                {
                    if (job.ResponseContentType != null)
                    {
                        MBApi.wkeNetSetMIMEType(job.Handle, job.ResponseContentType);
                    }

                    NetSetData(job.Handle, job.Data);
                    MBApi.wkeNetContinueJob(job.Handle);
                });
            }
            else
            {
                this.UIInvoke(() =>
                {
                    if (job.BeginArgs.HookRequest)
                    {
                        if (job.BeginArgs.IsLocalFile)
                        {
                            OnLoadUrlEnd(job.WebView, job.Handle);
                        }
                        else
                        {
                            MBApi.wkeNetHookRequest(job.Handle);
                        }
                    }

                    MBApi.wkeNetContinueJob(job.Handle);
                });
            }
        }

        protected virtual bool OnLoadUrlBegin(IntPtr mb, IntPtr param, IntPtr url, IntPtr job)
        {
            if (_loadUrlBegin == null) return false;

            var rawurl = url.ToUTF8String();
            var e = new LoadUrlBeginEventArgs
            {
                Job = new NetJob(mb, job, JobCompleted) {Url = rawurl},
                Url = rawurl,
                RequestMethod = MBApi.wkeNetGetRequestMethod(job)
            };
            e.Job.BeginArgs = e;

            _loadUrlBegin(this, e);

            if (e.Job.IsAsync)
            {
                return false;
            }

            if (e.HookRequest)
            {
                if (e.IsLocalFile)
                {
                    OnLoadUrlEnd(mb, job, e.Data);
                    return true;
                }

                MBApi.wkeNetHookRequest(job);
                return false;
            }

            if (e.Data != null)
            {
                NetSetData(job, e.Data);
                return true;
            }

            if (e.Cancel)
            {
                NetSetData(job);
            }

            return e.Cancel;
        }

        private void OnLoadUrlEnd(IntPtr mb, IntPtr param, IntPtr url, IntPtr job, IntPtr buf, int length)
        {
            var data = new byte[length];
            if (buf != IntPtr.Zero)
                Marshal.Copy(buf, data, 0, length);
            OnLoadUrlEnd(mb, job, data);
        }

        protected virtual void OnLoadUrlEnd(IntPtr webview, IntPtr job, byte[] data = null)
        {
            var begin = LoadUrlBeginEventArgs.GetByJob(job);
            if (begin == null || _wkeLoadUrlEnd == null) return;

            var end = begin.OnLoadUrlEnd(data);
            if (end.Modify)
            {
                NetSetData(job, end.Data);
            }
        }

        private static void NetSetData(IntPtr job, byte[] data = null)
        {
            if (data != null && data.Length > 0)
            {
                MBApi.wkeNetSetData(job, data, data.Length);
            }
            else
            {
                MBApi.wkeNetSetData(job, new byte[] {0}, 1);
            }
        }

        #endregion

        #region 公共方法

        public void ShowDevTools()
        {
            var path = Path.Combine(Application.StartupPath, "front_end", "inspector.html");
            MBApi.wkeShowDevtools(MiniblinkHandle, path, null, IntPtr.Zero);
        }

        public object RunJs(string script)
        {
            var es = MBApi.wkeGlobalExec(MiniblinkHandle);
            return MBApi.jsEvalExW(es, script, true).ToValue(es);
        }

        public object CallJsFunc(string funcName, params object[] param)
        {
            var es = MBApi.wkeGlobalExec(MiniblinkHandle);
            var func = MBApi.jsGetGlobal(es, funcName);
            if (func == 0)
                throw new WKEFunctionNotFondException(funcName);
            var args = param.Select(i => i.ToJsValue(es)).ToArray();
            return MBApi.jsCall(es, func, MBApi.jsUndefined(), args, args.Length).ToValue(es);
        }

        public void BindNetFunc(NetFunc func)
        {
            var funcvalue = new wkeJsNativeFunction((es, state) =>
            {
                var handle = GCHandle.FromIntPtr(state);
                var nfunc = (NetFunc) handle.Target;
                var arglen = MBApi.jsArgCount(es);
                var args = new List<object>();
                for (var i = 0; i < arglen; i++)
                {
                    args.Add(MBApi.jsArg(es, i).ToValue(es));
                }

                return nfunc.OnFunc(args.ToArray()).ToJsValue(es);
            });
            _ref[func.Name] = func;

            var ptr = GCHandle.ToIntPtr(GCHandle.Alloc(func));

            MBApi.wkeJsBindFunction(func.Name, funcvalue, ptr, 0);
        }

        public void SetHeadlessEnabled(bool enable)
        {
            MBApi.wkeSetHeadlessEnabled(MiniblinkHandle, enable);
        }

        public void SetNpapiPluginsEnable(bool enable)
        {
            MBApi.wkeSetNpapiPluginsEnabled(MiniblinkHandle, enable);
        }

        public void SetNavigationToNewWindow(bool enable)
        {
            MBApi.wkeSetNavigationToNewWindowEnable(MiniblinkHandle, enable);
        }

        public void SetCspCheckEnable(bool enable)
        {
            MBApi.wkeSetCspCheckEnable(MiniblinkHandle, enable);
        }

        public void SetTouchEnabled(bool enable)
        {
            MBApi.wkeSetTouchEnabled(MiniblinkHandle, enable);
        }

        public void SetMouseEnabled(bool enable)
        {
            MBApi.wkeSetMouseEnabled(MiniblinkHandle, enable);
        }

        public bool GoForward()
        {
            return MBApi.wkeGoForward(MiniblinkHandle);
        }

        public void EditorSelectAll()
        {
            MBApi.wkeEditorSelectAll(MiniblinkHandle);
        }

        public void EditorUnSelect()
        {
            MBApi.wkeEditorUnSelect(MiniblinkHandle);
        }

        public void EditorCopy()
        {
            MBApi.wkeEditorCopy(MiniblinkHandle);
        }

        public void EditorCut()
        {
            MBApi.wkeEditorCut(MiniblinkHandle);
        }

        public void EditorPaste()
        {
            MBApi.wkeEditorPaste(MiniblinkHandle);
        }

        public void EditorDelete()
        {
            MBApi.wkeEditorDelete(MiniblinkHandle);
        }

        public void EditorUndo()
        {
            MBApi.wkeEditorUndo(MiniblinkHandle);
        }

        public void EditorRedo()
        {
            MBApi.wkeEditorRedo(MiniblinkHandle);
        }

        public bool GoBack()
        {
            return MBApi.wkeGoBack(MiniblinkHandle);
        }

        public void SetProxy(WKEProxy proxy)
        {
            MBApi.wkeSetViewProxy(MiniblinkHandle, proxy);
        }

        public void LoadUri(string uri)
        {
            if (string.IsNullOrEmpty(uri?.Trim()))
                return;

            if (uri.SW("http:") || uri.SW("https:"))
            {
                MBApi.wkeLoadURL(MiniblinkHandle, uri);
            }
            else
            {
                MBApi.wkeLoadFileW(MiniblinkHandle, uri);
            }
        }

        public void LoadHtml(string html, string baseUrl = null)
        {
            if (baseUrl == null)
            {
                MBApi.wkeLoadHTML(MiniblinkHandle, html);
            }
            else
            {
                MBApi.wkeLoadHtmlWithBaseUrl(MiniblinkHandle, html, baseUrl);
            }
        }

        public void StopLoading()
        {
            MBApi.wkeStopLoading(MiniblinkHandle);
        }

        public void Reload()
        {
            MBApi.wkeReload(MiniblinkHandle);
        }

        #endregion

        internal static MiniblinkBrowser InvokeBro { get; private set; }
        private static string _hoolTipName = "func" + Guid.NewGuid().ToString().Replace("-", "");
        private static string _promptName = "func" + Guid.NewGuid().ToString().Replace("-", "");
        private EventHandler<PaintUpdatedEventArgs> _browserPaintUpdated;
        private Hashtable _ref = new Hashtable();
        public IList<ILoadResource> LoadResourceHandlerList { get; private set; }

        public MiniblinkBrowser()
        {
            InitializeComponent();

            InvokeBro = InvokeBro ?? this;
            LoadResourceHandlerList = new List<ILoadResource>();

            if (!Utils.IsDesignMode())
            {
                if (MBApi.wkeIsInitialize() == false)
                {
                    MBApi.wkeInitialize();
                }

                MiniblinkHandle = MBApi.wkeCreateWebView();

                if (MiniblinkHandle == IntPtr.Zero)
                {
                    throw new WKECreateException();
                }
                //_wndProcCallback = new WndProcCallback(OnWndMsg);
                //_bakWndProc = WinApi.SetWindowLong(Handle, (int)WinConst.GWL_WNDPROC, _wndProcCallback);
                //MBApi.wkeSetHandle(MiniblinkHandle, Handle);
                //MBApi.wkeSetDragEnable(MiniblinkHandle, false);
                //MBApi.wkeSetDragDropEnable(MiniblinkHandle, false);
                _browserPaintUpdated += BrowserPaintUpdated;
                var wkePaintUpdated = new wkePaintUpdatedCallback(OnPaintUpdated);
                _ref.Add(Guid.NewGuid(), wkePaintUpdated);
                MBApi.wkeOnPaintUpdated(MiniblinkHandle, wkePaintUpdated, Handle);

                LoadUrlBegin += LoadResource;
                DidCreateScriptContext += HookTip;
                DeviceParameter = new MBDeviceParameter(this);
                RegisterJsFunc();
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            LoadResourceHandlerList.Clear();
            _ref.Clear();
            base.OnHandleDestroyed(e);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!Utils.IsDesignMode() && !IsDisposed)
            {
                var hdc = MBApi.wkeGetViewDC(MiniblinkHandle);
                WinApi.BitBlt(e.Graphics.GetHdc(), e.ClipRectangle.X, e.ClipRectangle.Y,
                    e.ClipRectangle.Width, e.ClipRectangle.Height, hdc, e.ClipRectangle.X, e.ClipRectangle.Y,
                    (int)WinConst.SRCCOPY);
            }
        }

        private void BrowserPaintUpdated(object sender, PaintUpdatedEventArgs e)
        {
            if (!IsDisposed)
            {
                using (var gdi = CreateGraphics())
                {
                    WinApi.BitBlt(gdi.GetHdc(), e.X, e.Y, e.Width, e.Height, e.Hdc,
                        e.X, e.Y, (int) WinConst.SRCCOPY);
                }
            }
        }

        public void ScrollTo(int x, int y)
        {
            RunJs($"window.scrollTo({x},{y})");
        }

        public void RegisterNetFunc(object target)
        {
            var tg = target;
            var methods = tg.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<NetFuncAttribute>();
                if (attr == null) continue;
                BindNetFunc(new NetFunc(attr.Name ?? method.Name, ctx =>
                {
                    var m = (MethodInfo) ctx.State;
                    object ret;
                    var mps = m.GetParameters();
                    if (mps.Length < 1)
                    {
                        ret = m.Invoke(tg, null);
                    }
                    else
                    {
                        var param = ctx.Paramters;
                        var mpvs = new object[mps.Length];
                        for (var i = 0; i < mps.Length; i++)
                        {
                            var mp = mps[i];
                            var v = param.Length > i ? param[i] : null;
                            if (v != null)
                            {
                                var pt = mp.ParameterType;
                                if (pt.IsGenericType)
                                {
                                    if (pt.GetGenericTypeDefinition() == typeof(Nullable<>))
                                    {
                                        pt = pt.GetGenericArguments().First();
                                    }
                                }

                                if (pt == typeof(DateTime) && !(v is DateTime))
                                {
                                    long l_date;
                                    if (long.TryParse(v.ToString(), out l_date))
                                    {
                                        v = l_date.ToDate();
                                    }
                                }

                                if (v is JsFunc)
                                {
                                    mpvs[i] = v;
                                }
                                else
                                {
                                    mpvs[i] = Convert.ChangeType(v, pt);
                                }
                            }
                            else if (mp.ParameterType.IsValueType)
                            {
                                mpvs[i] = Activator.CreateInstance(mp.ParameterType);
                            }
                            else
                            {
                                mpvs[i] = null;
                            }
                        }

                        ret = m.Invoke(tg, mpvs);
                    }

                    return ret;
                }, method));
            }
        }

        private void LoadResource(object sender, LoadUrlBeginEventArgs e)
        {
            if (LoadResourceHandlerList.Count < 1)
                return;
            if (e.RequestMethod != wkeRequestType.Get)
                return;
            var url = e.Url;
            if (url.SW("http:") == false && url.SW("https:") == false)
                return;

            var uri = new Uri(url);
            byte[] data = null;

            foreach (var handler in LoadResourceHandlerList.ToArray())
            {
                data = handler.ByUri(uri);
                if (data != null)
                {
                    break;
                }
            }

            if (data != null)
            {
                e.IsLocalFile = true;
                e.Data = data;
            }
        }

        public void DrawToBitmap(Action<Image> callback)
        {
            new DrawToBitmapUtil(this).ToImage(callback);
        }

        private void RegisterJsFunc()
        {
            BindNetFunc(new NetFunc(_hoolTipName, OnHookTip));
            BindNetFunc(new NetFunc(_promptName, ShowPrompt));
        }

        private object ShowPrompt(NetFuncContext context)
        {
            var title = new Uri(Url).Host;
            string msg = null;
            string value = null;
            if (context.Paramters.Length > 0 && context.Paramters[0] != null)
            {
                msg = context.Paramters[0].ToString();
            }

            if (context.Paramters.Length > 1 && context.Paramters[1] != null)
            {
                value = context.Paramters[1].ToString();
            }

            var frm = new FrmPromptBox(title, msg, value);
            frm.ShowDialog();
            return frm.GetValue();
        }

        private void HookTip(object sender, DidCreateScriptContextEventArgs e)
        {
            e.Frame.RunJs(@"
                var abak=alert;
                alert=function(msg){
                    var data =" + _hoolTipName + @"('alert',msg);
                    if(data.cancel===false){
                        abak(data.msg);
                    }
                }
                var cbak=confirm;
                confirm=function(msg){
                    var data =" + _hoolTipName + @"('confirm',msg);
                    var rs = data.rs;
                    if(data.cancel===false){
                        rs = cbak(data.msg);
                        rs = data.rs||rs;
                    }
                    return rs;
                }
                prompt=function(msg,value){
                    var data =" + _hoolTipName + @"('prompt',msg,value);
                    var rs = data.rs;
                    if(data.cancel===false){
                        rs = " + _promptName + @"(data.msg,data.value);
                        rs = data.rs||rs;
                    }
                    return rs;
                }");
        }

        private object OnHookTip(NetFuncContext context)
        {
            var type = context.Paramters[0].ToString();
            switch (type)
            {
                case "alert":
                {
                    var ae = new AlertEventArgs();
                    if (context.Paramters.Length > 1 && context.Paramters[1] != null)
                    {
                        ae.Message = context.Paramters[1].ToString();
                    }

                    OnAlertBefore(ae);

                    return new
                    {
                        msg = ae.Message,
                        cancel = ae.Cancel
                    };
                }

                case "confirm":
                {
                    var ce = new ConfirmEventArgs();

                    if (context.Paramters.Length > 1 && context.Paramters[1] != null)
                    {
                        ce.Message = context.Paramters[1].ToString();
                    }

                    OnConfirmBefore(ce);

                    return new
                    {
                        cancel = ce.Cancel,
                        msg = ce.Message,
                        rs = ce.Result
                    };
                }

                case "prompt":
                {
                    var pe = new PromptEventArgs();

                    if (context.Paramters.Length > 1 && context.Paramters[1] != null)
                    {
                        pe.Message = context.Paramters[1].ToString();
                    }

                    if (context.Paramters.Length > 2 && context.Paramters[2] != null)
                    {
                        pe.Value = context.Paramters[2].ToString();
                    }

                    OnPromptBefore(pe);

                    return new
                    {
                        cancel = pe.Cancel,
                        msg = pe.Message,
                        rs = pe.Result,
                        value = pe.Value
                    };
                }

                default:
                    throw new ArgumentException("未知的参数值：" + type);
            }
        }

        public void Print(Action<PrintPreviewDialog> callback)
        {
            new PrintUtil(this).Start(callback);
        }

        public void OnDropFiles(int x, int y, string data)
        {
            data = data.Replace("\"", "\\\"").Replace("\\", "\\\\");
            x += ScrollLeft;
            y += ScrollTop;
            RunJs($@"
                var e = new CustomEvent(""dropData"",
                {{
                    detail:{{
                        data:""{data}"",
                        x:{x},
                        y:{y}
                    }}
                }});
                (window.dispatchEvent || window.fireEvent)(e);
            ");
        }

        protected override void OnResize(EventArgs e)
        {
            if (!Utils.IsDesignMode() && MiniblinkHandle != IntPtr.Zero)
            {
                MBApi.wkeResize(MiniblinkHandle, Width, Height);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            var code = e.KeyValue;
            var flags = (uint)wkeKeyFlags.WKE_REPEAT;

            if (Utils.IsExtendedKey(e.KeyCode))
            {
                flags |= (uint)wkeKeyFlags.WKE_EXTENDED;
            }

            if (MBApi.wkeFireKeyUpEvent(MiniblinkHandle, code, flags, false))
            {
                e.Handled = true;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            var code = e.KeyValue;
            var flags = (uint)wkeKeyFlags.WKE_REPEAT;

            if (Utils.IsExtendedKey(e.KeyCode))
            {
                flags |= (uint)wkeKeyFlags.WKE_EXTENDED;
            }

            if (MBApi.wkeFireKeyDownEvent(MiniblinkHandle, code, flags, false))
            {
                e.Handled = true;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            var code = e.KeyChar;
            var flags = (uint)wkeKeyFlags.WKE_REPEAT;

            if (MBApi.wkeFireKeyPressEvent(MiniblinkHandle, code, flags, false))
            {
                e.Handled = true;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            WinConst msg = 0;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    msg = WinConst.WM_LBUTTONDOWN;
                    break;
                case MouseButtons.Middle:
                    msg = WinConst.WM_MBUTTONDOWN;
                    break;
                case MouseButtons.Right:
                    msg = WinConst.WM_RBUTTONDOWN;
                    break;
            }

            if (msg != 0)
            {
                OnWkeMouseEvent(msg, e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            WinConst msg = 0;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    msg = WinConst.WM_LBUTTONUP;
                    break;
                case MouseButtons.Middle:
                    msg = WinConst.WM_MBUTTONUP;
                    break;
                case MouseButtons.Right:
                    msg = WinConst.WM_RBUTTONUP;
                    break;
            }

            if (msg != 0)
            {
                OnWkeMouseEvent(msg, e);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            WinConst msg = 0;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    msg = WinConst.WM_LBUTTONDBLCLK;
                    break;
                case MouseButtons.Middle:
                    msg = WinConst.WM_MBUTTONDBLCLK;
                    break;
                case MouseButtons.Right:
                    msg = WinConst.WM_RBUTTONDBLCLK;
                    break;
            }

            if (msg != 0)
            {
                OnWkeMouseEvent(msg, e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            OnWkeMouseEvent(WinConst.WM_MOUSEMOVE, e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Console.WriteLine("wheel");
            uint flags = 0;

            if (ModifierKeys.HasFlag(Keys.Control))
                flags |= (uint)wkeMouseFlags.WKE_CONTROL;
            if (ModifierKeys.HasFlag(Keys.LShiftKey))
                flags |= (uint)wkeMouseFlags.WKE_SHIFT;

            if (e.Button.HasFlag(MouseButtons.Left))
                flags |= (uint)wkeMouseFlags.WKE_LBUTTON;
            if (e.Button.HasFlag(MouseButtons.Middle))
                flags |= (uint)wkeMouseFlags.WKE_MBUTTON;
            if (e.Button.HasFlag(MouseButtons.Right))
                flags |= (uint)wkeMouseFlags.WKE_RBUTTON;

            MBApi.wkeFireMouseWheelEvent(MiniblinkHandle, e.X, e.Y, e.Delta, flags);
        }

        private void OnWkeMouseEvent(WinConst msg, MouseEventArgs e)
        {
            var flags = 0;

            if (ModifierKeys.HasFlag(Keys.Control))
                flags |= (int)wkeMouseFlags.WKE_CONTROL;
            if (ModifierKeys.HasFlag(Keys.LShiftKey))
                flags |= (int)wkeMouseFlags.WKE_SHIFT;

            if (e.Button.HasFlag(MouseButtons.Left))
                flags |= (int)wkeMouseFlags.WKE_LBUTTON;
            if (e.Button.HasFlag(MouseButtons.Middle))
                flags |= (int)wkeMouseFlags.WKE_MBUTTON;
            if (e.Button.HasFlag(MouseButtons.Right))
                flags |= (int)wkeMouseFlags.WKE_RBUTTON;

            MBApi.wkeFireMouseEvent(MiniblinkHandle, (int)msg, e.X, e.Y, flags);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            Focus();
        }

        private void SetWkeCursor()
        {
            MBApi.wkeFireWindowsMessage(MiniblinkHandle, Handle, (uint)WinConst.WM_SETCURSOR, IntPtr.Zero,
                IntPtr.Zero, IntPtr.Zero);
        }

        private void SetImeStartPos()
        {
            var caret = MBApi.wkeGetCaretRect(MiniblinkHandle);
            var comp = new CompositionForm
            {
                dwStyle = (int)WinConst.CFS_POINT | (int)WinConst.CFS_FORCE_POSITION,
                ptCurrentPos =
                {
                    x = caret.x,
                    y = caret.y
                }
            };
            var imc = WinApi.ImmGetContext(Handle);
            WinApi.ImmSetCompositionWindow(imc, ref comp);
            WinApi.ImmReleaseContext(Handle, imc);
        }


        protected override void WndProc(ref Message m)
        {
            switch ((WinConst) m.Msg)
            {
                case WinConst.WM_INPUTLANGCHANGE:
                {
                    DefWndProc(ref m);
                    break;
                }

                case WinConst.WM_IME_STARTCOMPOSITION:
                {
                    SetImeStartPos();
                    break;
                }

                case WinConst.WM_SETFOCUS:
                {
                    MBApi.wkeSetFocus(MiniblinkHandle);
                    break;
                }

                default:
                {
                    base.WndProc(ref m);
                    break;
                }
            }


            switch ((WinConst) m.Msg)
            {
                case WinConst.WM_SETCURSOR:
                {
                    SetWkeCursor();
                    break;
                }
            }
        }
    }
}