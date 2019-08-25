using System;
using System.Drawing;

namespace QQ2564874169.Miniblink
{
	public interface IMiniblink
	{
        IntPtr MiniblinkHandle { get; }
		string Url { get; }
		bool IsDocumentReady { get; }
		string DocumentTitle { get; }
		int DocumentWidth { get; }
		int DocumentHeight { get; }
		int ContentWidth { get; }
		int ContentHeight { get; }
        int ViewWidth { get; }
        int ViewHeight { get; }
        int ScrollTop { get; set; }
        int ScrollLeft { get; set; }
        int ScrollHeight { get; }
        int ScrollWidth { get; }
		bool CanGoBack { get; }
		bool CanGoForward { get; }
		float Zoom { get; set; }
		bool CookieEnabled { get; set; }
		string UserAgent { get; set; }
        MBDeviceParameter DeviceParameter { get; }

        event EventHandler<UrlChangedEventArgs> UrlChanged;
		event EventHandler<NavigateEventArgs> NavigateBefore;
		event EventHandler<DocumentReadyEventArgs> DocumentReady;
		event EventHandler<ConsoleMessageEventArgs> ConsoleMessage;
		event EventHandler<NetResponseEventArgs> NetResponse;
		event EventHandler<LoadUrlBeginEventArgs> LoadUrlBegin;
        event EventHandler<WndMsgEventArgs> WndMsg;
        event EventHandler<PaintUpdatedEventArgs> PaintUpdated;
        event EventHandler<DownloadEventArgs> Download;
        event EventHandler<AlertEventArgs> AlertBefore;
        event EventHandler<ConfirmEventArgs> ConfirmBefore;
        event EventHandler<PromptEventArgs> PromptBefore;
	    event EventHandler<DidCreateScriptContextEventArgs> DidCreateScriptContext;

        void ScrollTo(int x, int y);
		void RegisterNetFunc(object target);
		void ShowDevTools();
		object RunJs(string script);
		object CallJsFunc(string funcName, params object[] param);
		void BindNetFunc(NetFunc func);
		void SetHeadlessEnabled(bool enable);
		void SetNpapiPluginsEnable(bool enable);
		void SetNavigationToNewWindow(bool enable);
		void SetCspCheckEnable(bool enable);
        void SetTouchEnabled(bool enable);
        void SetMouseEnabled(bool enable);
        bool GoForward();
		void EditorSelectAll();
		void EditorUnSelect();
		void EditorCopy();
		void EditorCut();
		void EditorPaste();
		void EditorDelete();
		void EditorUndo();
		void EditorRedo();
		bool GoBack();
		void SetProxy(WKEProxy proxy);
		void LoadUri(string uri);
		void LoadHtml(string html, string baseUrl = null);
		void StopLoading();
		void Reload();
        void DrawToBitmap(Action<Image> callback);
    }
}
