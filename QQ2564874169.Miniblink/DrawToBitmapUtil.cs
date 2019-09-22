using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace QQ2564874169.Miniblink
{
    public class DrawToBitmapUtil
    {
        private MiniblinkBrowser _miniblink;
        private Action<ScreenshotImage> _imgCallback;
        private List<Image> _images = new List<Image>();
        private int _scrollTopBak;
        private int _imageHeight;
        private int _contentHeight;
        private int _contentWidth;
        private string _cssBak;

        public DrawToBitmapUtil(MiniblinkBrowser miniblink)
        {
            _miniblink = miniblink;
        }

        public void ToImage(Action<ScreenshotImage> callback)
        {
            _imgCallback = callback;
            _cssBak = _miniblink.RunJs(@"
                var b = document.getElementsByTagName('body')[0];
                var v = b.style.overflow;
                b.style.overflow='hidden';
                return v;").ToString();
            _contentHeight = _miniblink.ScrollHeight;
            _contentWidth = _miniblink.ContentWidth;
            _imageHeight = Screen.PrimaryScreen.WorkingArea.Height;
            if (_contentHeight < _imageHeight)
            {
                _imageHeight = _contentHeight;
            }
            _scrollTopBak = _miniblink.ScrollTop;
            _miniblink.ScrollTop = 0;
            _miniblink.PaintUpdated += WaitToImagePaint;
            MBApi.wkeResize(_miniblink.MiniblinkHandle, _contentWidth, _imageHeight);
        }

        private void WaitToImagePaint(object sender, PaintUpdatedEventArgs e)
        {
            e.Cancel = true;
            var w = MBApi.wkeGetWidth(_miniblink.MiniblinkHandle);
            var h = MBApi.wkeGetHeight(_miniblink.MiniblinkHandle);
            if (w != _contentWidth || h != _imageHeight)
                return;

            var height = _imageHeight;
            var scrTop = _imageHeight;
            var srcY = 0;
            var isLast = false;
            if ((_images.Count + 1) * _imageHeight >= _contentHeight)
            {
                height = _contentHeight - _images.Count * _imageHeight;
                scrTop = height;
                srcY = _imageHeight - height;
                isLast = true;
            }

            var bmp = new Bitmap(_contentWidth, height);

            using (var g = Graphics.FromImage(bmp))
            {
                WinApi.BitBlt(g.GetHdc(), 0, 0, bmp.Width, bmp.Height,
                    MBApi.wkeGetViewDC(_miniblink.MiniblinkHandle), 0, srcY,
                    (int) WinConst.SRCCOPY);
            }

            _images.Add(bmp);

            if (isLast)
            {
                _miniblink.PaintUpdated -= WaitToImagePaint;
                _miniblink.ScrollTop = _scrollTopBak;
                _miniblink.RunJs($"document.getElementsByTagName('body')[0].style.overflow='{_cssBak}'");
                _miniblink.PaintUpdated += DisablePaintUpdated;
                MBApi.wkeResize(_miniblink.MiniblinkHandle, _miniblink.Width, _miniblink.Height);
                using (var ss = new ScreenshotImage(_images))
                {
                    _imgCallback?.Invoke(ss);
                }
                _images.Clear();
                _imgCallback = null;
            }
            else
            {
                _miniblink.ScrollTop += scrTop;
            }
        }

        private void DisablePaintUpdated(object sender, PaintUpdatedEventArgs e)
        {
            _miniblink.PaintUpdated -= DisablePaintUpdated;
            e.Cancel = true;
        }
    }
}
