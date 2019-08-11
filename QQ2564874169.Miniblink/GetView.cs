using System;
using System.Drawing;

namespace QQ2564874169.Miniblink
{
    public class GetView
    {
        private MiniblinkBrowser _miniblink;
        private Action<Image> _callback;

        public GetView(MiniblinkBrowser miniblink)
        {
            _miniblink = miniblink;
        }

        public void ToImage(Action<Image> callback)
        {
            _callback = callback;
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
                _callback?.Invoke(bmp);
            }
        }

        private void DisablePaintUpdated(object sender, PaintUpdatedEventArgs e)
        {
            _miniblink.PaintUpdated -= DisablePaintUpdated;
            e.Cancel = true;
        }
    }
}
