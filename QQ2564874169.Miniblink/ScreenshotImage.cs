using System;
using System.Collections.Generic;
using System.Drawing;

namespace QQ2564874169.Miniblink
{
    public class ScreenshotImage : IDisposable
    {
        public int Width { get; set; }
        public int Height { get; set; }

        private List<Image> _data = new List<Image>();

        internal ScreenshotImage(IEnumerable<Image> images)
        {
            _data.AddRange(images);
        }

        public Image GetImage(int x, int y, int width, int height)
        {
            return null;
        }

        public void Dispose()
        {
            if (_data != null)
            {
                foreach (var item in _data)
                {
                    item.Dispose();
                }
                _data.Clear();
                _data = null;
            }
        }
    }
}
