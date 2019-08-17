using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace QQ2564874169.Miniblink
{
    public class GetView
    {
        private MiniblinkBrowser _miniblink;
        private Action<Image> _imgCallback;
        private Action<Stream> _smCallback;

        public GetView(MiniblinkBrowser miniblink)
        {
            _miniblink = miniblink;
        }

        public void ToImage(Action<Image> callback)
        {
            _imgCallback = callback;
            _miniblink.PaintUpdated += ToImagePaint;
            MBApi.wkeResize(_miniblink.MiniblinkHandle, _miniblink.ContentWidth, _miniblink.ContentHeight);
        }

        private void ToImagePaint(object sender, PaintUpdatedEventArgs e)
        {
            e.Cancel = true;
            _miniblink.PaintUpdated -= ToImagePaint;
            var w = _miniblink.ContentWidth;
            var h = _miniblink.ContentHeight;
            using (var bmp = new Bitmap(w, h))
            {
                using (var gdi = Graphics.FromImage(bmp))
                {
                    WinApi.BitBlt(gdi.GetHdc(), 0, 0, bmp.Width, bmp.Height,
                        MBApi.wkeGetViewDC(_miniblink.MiniblinkHandle), 0, 0,
                        (int)WinConst.SRCCOPY);
                }

                _miniblink.PaintUpdated += DisablePaintUpdated;
                MBApi.wkeResize(_miniblink.MiniblinkHandle, _miniblink.Width, _miniblink.Height);
                _imgCallback?.Invoke(bmp);
                _imgCallback = null;
            }
        }

        public void ToStream(Action<Stream> callback)
        {
            _smCallback = callback;
            _miniblink.PaintUpdated += ToStreamPaint;
            MBApi.wkeResize(_miniblink.MiniblinkHandle, _miniblink.ContentWidth, _miniblink.ContentHeight);
        }

        private void ToStreamPaint(object sender, PaintUpdatedEventArgs e)
        {
            e.Cancel = true;
            _miniblink.PaintUpdated -= ToStreamPaint;
            var w = _miniblink.ContentWidth;
            var h = _miniblink.ContentHeight;

            using (var ms = new MemoryStream())
            {
                var buf = new byte[w * h * 4];
                MBApi.wkePaint(_miniblink.MiniblinkHandle, buf, 0);
                ms.Write(buf, 0, buf.Length);
                ms.Position = 0;
                _miniblink.PaintUpdated += DisablePaintUpdated;
                MBApi.wkeResize(_miniblink.MiniblinkHandle, _miniblink.Width, _miniblink.Height);
                _smCallback?.Invoke(ms);
            }
        }

        private void DisablePaintUpdated(object sender, PaintUpdatedEventArgs e)
        {
            _miniblink.PaintUpdated -= DisablePaintUpdated;
            e.Cancel = true;
        }
    }
}
