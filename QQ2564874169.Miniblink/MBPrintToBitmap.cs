using System;
using System.Collections.Generic;
using System.Drawing;

namespace QQ2564874169.Miniblink
{
    public class MBPrintToBitmap
    {
        private int _scrollTop;
        private int _scrollLeft;
        private int _index;
        private List<Rectangle> _points = new List<Rectangle>();
        private Dictionary<string, Image> _images = new Dictionary<string, Image>();
        private MiniblinkBrowser _miniblink;
        private Action<Image> _callback;
        private int _contentWidth;
        private int _contentHeight;
        private bool _printing;
        private bool _reset;

        public MBPrintToBitmap(MiniblinkBrowser miniblink)
        {
            _miniblink = miniblink;
        }

        public void Start(Action<Image> callback)
        {
            _reset = false;
            _printing = true;
            _callback = callback;
            _scrollLeft = _miniblink.ScrollLeft;
            _scrollTop = _miniblink.ScrollTop;
            var dw = _miniblink.DocumentWidth;
            var dh = _miniblink.DocumentHeight;
            var cw = _contentWidth = _miniblink.ContentWidth;
            var ch = _contentHeight = _miniblink.ContentHeight;
            dw = Math.Min(dw, cw);
            dh = Math.Min(dh, ch);
            var w = cw / dw + 1;
            var h = ch / dh + 1;

            _miniblink.PaintUpdated += DisableRefresh;
            _miniblink.ScrollTo(cw, ch);

            for (var i = 0; i < h; i++)
            {
                var y = i * dh;

                for (var j = 0; j < w; j++)
                {
                    var x = j * dw;

                    if (x + dw > cw)
                    {
                        x = cw - dw;
                        if (x == 0)
                        {
                            continue;
                        }
                    }

                    if (y + dh > ch)
                    {
                        y = ch - dh;
                        if (y == 0)
                        {
                            continue;
                        }
                    }

                    _points.Add(new Rectangle(x, y, dw, dh));
                }
            }

            _miniblink.PaintUpdated += _miniblink_PaintUpdated;
            _miniblink.ScrollTo(0, 0);
        }

        private void DisableRefresh(object sender, PaintUpdatedEventArgs e)
        {
            if (_printing)
            {
                e.Cancel = true;
            }
        }

        private void _miniblink_PaintUpdated(object sender, PaintUpdatedEventArgs e)
        {
            if (_printing == false)
            {
                return;
            }
            if (_index < _points.Count)
            {
                var rect = _points[_index];
                var bmp = new Bitmap(rect.Width, rect.Height);
                using (var gdi = Graphics.FromImage(bmp))
                {
                    WinApi.BitBlt(gdi.GetHdc(), 0, 0, bmp.Width, bmp.Height,
                        MBApi.wkeGetViewDC(_miniblink.MiniblinkHandle), 0, 0,
                        (int) WinConst.SRCCOPY);
                }

                _images[$"{rect.X}-{rect.Y}"] = bmp;
                _index++;
            }

            if (_index < _points.Count)
            {
                var rect = _points[_index];
                _miniblink.ScrollTo(rect.X, rect.Y);
            }
            else if(_reset == false)
            {
                _reset = true;
                _miniblink.ScrollTo(_scrollLeft, _scrollTop);
                
            }
            else
            {
                _reset = false;
                _miniblink.PaintUpdated -= DisableRefresh;
                _miniblink.PaintUpdated -= _miniblink_PaintUpdated;
                Finish();
            }
        }

        private void Finish()
        {
            using (var bmp = new Bitmap(_contentWidth, _contentHeight))
            {
                using (var gdi = Graphics.FromImage(bmp))
                {
                    foreach (var pos in _points)
                    {
                        var key = $"{pos.X}-{pos.Y}";
                        if (_images.ContainsKey(key))
                        {
                            using (var img = _images[key])
                            {
                                gdi.DrawImage(img, pos.Location);
                            }

                            _images.Remove(key);
                        }
                    }
                }

                _printing = false;
                _index = 0;
                _points.Clear();
                _images.Clear();
                _callback?.Invoke(bmp);
            }
        }
    }
}
