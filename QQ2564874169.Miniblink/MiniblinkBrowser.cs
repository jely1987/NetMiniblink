﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
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
        public DeviceParameter DeviceParameter { get; }

        private bool _fireDropFile;

        public bool FireDropFile
        {
            get { return _fireDropFile; }
            set
            {
                if (_fireDropFile == value)
                {
                    return;
                }

                if (value)
                {
                    DragDrop += DragFileDrop;
                    DragEnter += DragFileEnter;
                }
                else
                {
                    DragDrop -= DragFileDrop;
                    DragEnter -= DragFileEnter;
                }

                _fireDropFile = value;
            }
        }

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

        public event EventHandler<WindowOpenEventArgs> WindowOpen;

        protected virtual WindowOpenEventArgs OnWindowOpen(string url, string name,
            IDictionary<string, string> specs, bool replace)
        {
            var args = new WindowOpenEventArgs
            {
                Name = name,
                Url = url,
                Replace = replace
            };
            if (specs != null)
            {
                foreach (var item in specs)
                {
                    args.Specs.Add(item);
                }
            }

            WindowOpen?.Invoke(this, args);

            if (args.LoadUrl && url != null)
            {
                LoadUri(url);
            }

            return args;
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
                Url = url.WKEToUTF8String()
            };
            switch (type)
            {
                case wkeNavigationType.BackForward:
                    e.Type = NavigateType.BackForward;
                    break;
                case wkeNavigationType.FormReSubmit:
                    e.Type = NavigateType.ReSubmit;
                    break;
                case wkeNavigationType.FormSubmit:
                    e.Type = NavigateType.Submit;
                    break;
                case wkeNavigationType.LinkClick:
                    e.Type = NavigateType.LinkClick;
                    break;
                case wkeNavigationType.ReLoad:
                    e.Type = NavigateType.ReLoad;
                    break;
                case wkeNavigationType.Other:
                    e.Type = NavigateType.Other;
                    break;
                default:
                    throw new Exception("未知的重定向类型：" + type);
            }
            OnNavigateBefore(e);

            return (byte) (e.Cancel ? 0 : 1);
        }

        protected virtual void OnNavigateBefore(NavigateEventArgs args)
        {
            _navigateBefore(this, args);
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
            _documentReady?.Invoke(this, new DocumentReadyEventArgs
            {
                Frame = new FrameContext(this, frameId)
            });
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
            if (_loadUrlBegin == null)
                return false;
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

            if (e.Data != null)
            {
                if (!e.HookRequest || !OnLoadUrlEnd(mb, job, e.Data))
                {
                    NetSetData(job, e.Data);
                }
                MBApi.wkeNetCancelRequest(job);
                return true;
            }

            if (e.HookRequest)
            {
                if (e.IsLocalFile)
                {
                    OnLoadUrlEnd(mb, job, e.Data);
                    MBApi.wkeNetCancelRequest(job);
                    return true;
                }
                
                MBApi.wkeNetHookRequest(job);
                return false;
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

        protected virtual bool OnLoadUrlEnd(IntPtr mb, IntPtr job, byte[] data = null)
        {
            var begin = LoadUrlBeginEventArgs.GetByJob(job);
            if (begin != null)
            {
                var end = begin.OnLoadUrlEnd(data);
                if (end.Modify || begin.IsLocalFile)
                {
                    NetSetData(job, end.Data);
                    return true;
                }
            }
            return false;
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
        private static string _popHookName = "func" + Guid.NewGuid().ToString().Replace("-", "");
        private static string _openHookName = "func" + Guid.NewGuid().ToString().Replace("-", "");
        private EventHandler<PaintUpdatedEventArgs> _browserPaintUpdated;
        private Hashtable _ref = new Hashtable();
        public IList<ILoadResource> LoadResourceHandlerList { get; private set; }
        public IResourceCache ResourceCache { get; set; }
        public CookieCollection Cookies => GetCookies();

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
                MBApi.wkeSetDragEnable(MiniblinkHandle, false);
                MBApi.wkeSetDragDropEnable(MiniblinkHandle, false);
                MBApi.wkeSetHandle(MiniblinkHandle, Handle);
                MBApi.wkeSetNavigationToNewWindowEnable(MiniblinkHandle, true);
                MBApi.wkeOnCreateView(MiniblinkHandle, OnCreateView, IntPtr.Zero);
                _browserPaintUpdated += BrowserPaintUpdated;
                var wkePaintUpdated = new wkePaintUpdatedCallback(OnPaintUpdated);
                _ref.Add(Guid.NewGuid(), wkePaintUpdated);
                MBApi.wkeOnPaintUpdated(MiniblinkHandle, wkePaintUpdated, Handle);

                LoadUrlBegin += LoadResource;
                DidCreateScriptContext += HookPop;
                DeviceParameter = new DeviceParameter(this);
                RegisterJsFunc();
                
            }
        }

        private IntPtr OnCreateView(IntPtr mb, IntPtr param, wkeNavigationType type, IntPtr url, IntPtr windowFeatures)
        {
            if (type == wkeNavigationType.LinkClick)
            {
                var e = new NavigateEventArgs
                {
                    Url = url.ToUTF8String(),
                    Type = NavigateType.BlankLink
                };
                OnNavigateBefore(e);
            }
            else
            {
                OnNavigateBefore(mb, param, type, url);
            }

            return IntPtr.Zero;
        }

        private void DestroyCallback()
        {
            MBApi.wkeOnPaintUpdated(MiniblinkHandle, null, IntPtr.Zero);
            MBApi.wkeOnURLChanged2(MiniblinkHandle, null, IntPtr.Zero);
            MBApi.wkeOnNavigation(MiniblinkHandle, null, IntPtr.Zero);
            MBApi.wkeOnDocumentReady2(MiniblinkHandle, null, IntPtr.Zero);
            MBApi.wkeOnConsole(MiniblinkHandle, null, IntPtr.Zero);
            MBApi.wkeNetOnResponse(MiniblinkHandle, null, IntPtr.Zero);
            MBApi.wkeOnLoadUrlBegin(MiniblinkHandle, null, IntPtr.Zero);
            MBApi.wkeOnLoadUrlEnd(MiniblinkHandle, null, IntPtr.Zero);
            MBApi.wkeOnDownload(MiniblinkHandle, null, IntPtr.Zero);
            MBApi.wkeOnCreateView(MiniblinkHandle, null, IntPtr.Zero);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            DestroyCallback();
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
            if (IsDocumentReady)
            {
                RunJs($"window.scrollTo({x},{y})");
            }
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

                                if (v is JsFunc || pt == typeof(object) || pt == typeof(ExpandoObject))
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

            foreach (var handler in LoadResourceHandlerList.ToArray())
            {
                if (handler.Domain.Equals(uri.Host, StringComparison.OrdinalIgnoreCase) == false)
                    continue;
                e.IsLocalFile = true;
                var data = handler.ByUri(uri);
                if (data != null)
                {
                    e.Data = data;
                    break;
                }
            }
        }

        public void DrawToBitmap(Action<ScreenshotImage> callback)
        {
            new DrawToBitmapUtil(this).ToImage(callback);
        }

        private void RegisterJsFunc()
        {
            BindNetFunc(new NetFunc(_popHookName, OnHookPop));
            BindNetFunc(new NetFunc(_openHookName, OnHookWindowOpen));
        }

        private void HookPop(object sender, DidCreateScriptContextEventArgs e)
        {
            var js = string.Join(".", GetType().Namespace, "Files", "hook.js");

            using (var sm = GetType().Assembly.GetManifestResourceStream(js))
            {
                if (sm != null)
                {
                    using (var reader = new StreamReader(sm, Encoding.UTF8))
                    {
                        js = reader.ReadToEnd();
                    }
                }
            }

            js = $@"var popHookName='{_popHookName}';
                    var openHookName='{_openHookName}';"
                 + js;

            e.Frame.RunJs(js);
        }

        private AlertEventArgs OnAlert(string message, string title)
        {
            var args = new AlertEventArgs
            {
                Window = new FrmAlert
                {
                    Message = message,
                    Text = title
                }
            };
            OnAlertBefore(args);
            args.Window?.ShowDialog();
            return args;
        }

        private ConfirmEventArgs OnConfirm(string message, string title)
        {
            var args = new ConfirmEventArgs
            {
                Window = new FrmConfirm
                {
                    Message = message,
                    Text = title
                }
            };
            OnConfirmBefore(args);
            args.Window?.ShowDialog();
            if (args.Result.HasValue == false && args.Window != null)
            {
                args.Result = args.Window.IsOk;
            }

            return args;
        }

        private PromptEventArgs OnPrompt(string message, string input, string title)
        {
            var args = new PromptEventArgs
            {
                Window = new FrmPrompt
                {
                    Text = title,
                    Message = message,
                    Value = input
                }
            };
            OnPromptBefore(args);
            args.Window?.ShowDialog();
            if (args.Result == null && args.Window != null)
            {
                args.Result = args.Window.Value;
            }

            return args;
        }

        private object OnHookPop(NetFuncContext context)
        {
            if (context.Paramters.Length < 1)
            {
                return null;
            }
            var type = context.Paramters[0].ToString().ToLower();

            if ("alert" == type)
            {
                var msg = "";
                var title = new Uri(Url).Host;
                if (context.Paramters.Length > 1 && context.Paramters[1] != null)
                {
                    msg = context.Paramters[1].ToString();
                }

                if (context.Paramters.Length > 2 && context.Paramters[2] != null)
                {
                    dynamic opt = context.Paramters[2];
                    if (opt.title != null)
                    {
                        title = opt.title.ToString();
                    }
                }

                OnAlert(msg, title);
                return null;
            }

            if ("confirm" == type)
            {
                var msg = "";
                var title = new Uri(Url).Host;
                if (context.Paramters.Length > 1 && context.Paramters[1] != null)
                {
                    msg = context.Paramters[1].ToString();
                }

                if (context.Paramters.Length > 2 && context.Paramters[2] != null)
                {
                    dynamic opt = context.Paramters[2];
                    if (opt.title != null)
                    {
                        title = opt.title.ToString();
                    }
                }

                var e = OnConfirm(msg, title);
                return e.Result.GetValueOrDefault();
            }

            if ("prompt" == type)
            {
                var msg = "";
                var input = "";
                var title = new Uri(Url).Host;
                if (context.Paramters.Length > 1 && context.Paramters[1] != null)
                {
                    msg = context.Paramters[1].ToString();
                }

                if (context.Paramters.Length > 2 && context.Paramters[2] != null)
                {
                    input = context.Paramters[2].ToString();
                }

                if (context.Paramters.Length > 3 && context.Paramters[3] != null)
                {
                    dynamic opt = context.Paramters[3];
                    if (opt.title != null)
                    {
                        title = opt.title.ToString();
                    }
                }

                var e = OnPrompt(msg, input, title);
                return e.Result;
            }

            return null;
        }

        private object OnHookWindowOpen(NetFuncContext context)
        {
            string url = null;
            string name = null;
            string specs = null;
            string replace = null;
            var map = new Dictionary<string, string>();

            if (context.Paramters.Length > 0 && context.Paramters[0] != null)
            {
                url = context.Paramters[0].ToString();
            }

            if (context.Paramters.Length > 1 && context.Paramters[1] != null)
            {
                name = context.Paramters[1].ToString();
            }

            if (context.Paramters.Length > 2 && context.Paramters[2] != null)
            {
                specs = context.Paramters[2].ToString();
            }

            if (context.Paramters.Length > 3 && context.Paramters[3] != null)
            {
                replace = context.Paramters[3].ToString();
            }

            if (specs != null)
            {
                var items = specs.Split(',');
                foreach (var item in items)
                {
                    var kv = item.Split('=');
                    if (kv.Length == 1)
                    {
                        map[kv[0]] = "";
                    }
                    else if (kv.Length == 2)
                    {
                        map[kv[0]] = kv[1];
                    }
                }
            }

            var navArgs = new NavigateEventArgs
            {
                Url = url,
                Type = NavigateType.WindowOpen
            };
            OnNavigateBefore(navArgs);
            if (navArgs.Cancel)
            {
                return null;
            }
            var e = OnWindowOpen(url, name, map, "true" == replace);
            return e.ReturnValue;
        }

        public void Print(Action<PrintPreviewDialog> callback)
        {
            new PrintUtil(this).Start(callback);
        }

        public CookieCollection GetCookies(string domain)
        {
            throw new NotImplementedException();
        }

        private void OnDropFiles(int x, int y, params string[] files)
        {
            for (var i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Replace("\"", "\\\"").Replace("\\", "\\\\");
                files[i] = "\"" + files[i] + "\"";
            }

            var data = string.Join(",", files);
            x += ScrollLeft;
            y += ScrollTop;
            RunJs($@"
                var e = new CustomEvent(""dropFile"",
                {{
                    detail:{{
                        files:[{data}],
                        x:{x},
                        y:{y}
                    }}
                }});
                (window.dispatchEvent || window.fireEvent)(e);
            ");
        }

        private void DragFileEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void DragFileDrop(object sender, DragEventArgs e)
        {
            var items = (Array) e.Data.GetData(DataFormats.FileDrop);
            var files = items.Cast<string>().ToArray();
            var p = PointToClient(new Point(e.X, e.Y));
            OnDropFiles(p.X, p.Y, files);
        }

        private CookieCollection GetCookies()
        {
            var file = "cookies.dat";

            if (File.Exists(file) == false)
            {
                return null;
            }
            MBApi.wkePerformCookieCommand(MiniblinkHandle, wkeCookieCommand.FlushCookiesToFile);
            var host = new Uri(Url).Host.ToLower();
            var cookies = new CookieCollection();
            var rows = File.ReadAllLines(file, Encoding.UTF8);

            foreach (var row in rows)
            {
                if (row.StartsWith("# ")) continue;
                var items = row.Split('\t');
                if (items.Length != 7) continue;
                var domain = items[0];
                var httpOnly = domain.StartsWith("#HttpOnly_");
                if (httpOnly)
                {
                    domain = domain.Substring(domain.IndexOf("_", StringComparison.Ordinal) + 1).ToLower();
                }

                if ("true".Equals(items[1], StringComparison.OrdinalIgnoreCase))
                {
                    if (host.EndsWith(domain) == false)
                    {
                        if (("." + host).Equals(domain) == false)
                        {
                            continue;
                        }
                    }
                }
                else if (host.Equals(domain) == false)
                {
                    continue;
                }

                var cookie = new Cookie
                {
                    HttpOnly = httpOnly,
                    Domain = domain.TrimStart('.'),
                    Path = items[2],
                    Secure = "true".Equals(items[3], StringComparison.OrdinalIgnoreCase),
                    Expires = new DateTime(1970, 1, 1).AddSeconds(long.Parse(items[4])),
                    Name = items[5],
                    Value = items[6]
                };
                cookies.Add(cookie);
            }

            return cookies;
        }

        #region 消息处理
        protected override void OnResize(EventArgs e)
        {
            if (!Utils.IsDesignMode() && MiniblinkHandle != IntPtr.Zero)
            {
                MBApi.wkeResize(MiniblinkHandle, Width, Height);
            }
            base.OnResize(e);
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
            base.OnKeyUp(e);
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
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            var code = e.KeyChar;
            var flags = (uint)wkeKeyFlags.WKE_REPEAT;

            if (MBApi.wkeFireKeyPressEvent(MiniblinkHandle, code, flags, false))
            {
                e.Handled = true;
            }
            base.OnKeyPress(e);
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
            base.OnMouseDown(e);
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
            base.OnMouseUp(e);
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
            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            OnWkeMouseEvent(WinConst.WM_MOUSEMOVE, e);
            base.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
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
            base.OnMouseWheel(e);
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

        private void SetWkeCursor()
        {
            var type = MBApi.wkeGetCursorInfoType(MiniblinkHandle);
            switch (type)
            {
                case wkeCursorInfo.Hand:
                    if (Cursor != Cursors.Hand)
                    {
                        Cursor = Cursors.Hand;

                    }
                    break;
                case wkeCursorInfo.IBeam:
                    if (Cursor != Cursors.IBeam)
                    {
                        Cursor = Cursors.IBeam;
                    }
                    break;
                case wkeCursorInfo.Pointer:
                    if (Cursor != Cursors.Default)
                    {
                        Cursor = Cursors.Default;
                    }
                    break;
            }
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

                case WinConst.WM_SETCURSOR:
                {
                    SetWkeCursor();
                    base.WndProc(ref m);
                    break;
                }

                default:
                {
                    base.WndProc(ref m);
                    break;
                }
            }
        }

        #endregion
    }
}