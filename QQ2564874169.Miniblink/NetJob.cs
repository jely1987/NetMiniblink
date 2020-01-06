using System;
using System.Threading.Tasks;

namespace QQ2564874169.Miniblink
{
    public class NetJob
    {
        public IntPtr Handle { get; }
        public IntPtr WebView { get; }
        public object State { get; private set; }
        public byte[] Data { get; set; }
        public string ResponseContentType { get; set; }
		public string Url { get; internal set; }

        internal bool IsAsync;
        internal LoadUrlBeginEventArgs Begin { get; }
        private Action<NetJob> _completed;

        internal NetJob(IntPtr webview, LoadUrlBeginEventArgs begin, IntPtr job, Action<NetJob> completed = null)
        {
            Handle = job;
            WebView = webview;
            Begin = begin;
            Url = begin.Url;
            _completed = completed;
        }

        public void Wait(Action<NetJob> callback, object state = null)
	    {
			if(Begin.Ended)
				return;

		    IsAsync = true;
		    State = state;
		    MBApi.wkeNetHoldJobToAsynCommit(Handle);

		    Task.Factory.StartNew(() =>
		    {
			    try
			    {
				    callback(this);
			    }
			    finally
			    {
				    _completed?.Invoke(this);
			    }
		    });
	    }

	    public void WatchLoadUrlEnd(Action<LoadUrlEndArgs> callback, object state = null)
	    {
            Begin.Response(callback);
	    }
    }
}
