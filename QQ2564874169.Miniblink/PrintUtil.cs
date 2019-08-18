using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QQ2564874169.Miniblink
{
    internal class PrintUtil
    {
        private IMiniblink _miniblink;
        private int _bakWidth;
        private int _bakHeight;
        private int _preY;
        private int _width;
        private int _height;
        private Action<PrintPreviewDialog> _callback;

        public PrintUtil(IMiniblink miniblink)
        {
            _miniblink = miniblink;
        }

        public void Start(Action<PrintPreviewDialog> callback)
        {
            _callback = callback;
            _bakWidth = _miniblink.ViewWidth;
            _bakHeight = _miniblink.ViewHeight;
            _width = _miniblink.ContentWidth;
            _height = _miniblink.ContentHeight;
            _miniblink.PaintUpdated += WaitPaint;
            MBApi.wkeResize(_miniblink.MiniblinkHandle, _width, _height);
        }

        private void DisablePaintUpdated(object sender, PaintUpdatedEventArgs e)
        {
            e.Cancel = true;
        }

        private void OnceDisablePaintUpdated(object sender, PaintUpdatedEventArgs e)
        {
            _miniblink.PaintUpdated -= OnceDisablePaintUpdated;
            e.Cancel = true;
        }

        private void WaitPaint(object sender, PaintUpdatedEventArgs e)
        {
            e.Cancel = true;
            _miniblink.PaintUpdated -= WaitPaint;
            _miniblink.PaintUpdated += DisablePaintUpdated;
            var pre = CreatePrintPreviewDialog();
            pre.Document.BeginPrint += Document_BeginPrint;
            pre.Document.PrintPage += Document_PrintPage;
            _callback?.Invoke(pre);
        }

        private PrintPreviewDialog CreatePrintPreviewDialog()
        {
            var pre = new PrintPreviewDialog { Document = new PrintDocument() };
            pre.Closed += (s, e) =>
            {
                _miniblink.PaintUpdated -= DisablePaintUpdated;
                _miniblink.PaintUpdated += OnceDisablePaintUpdated;
                MBApi.wkeResize(_miniblink.MiniblinkHandle, _bakWidth, _bakHeight);
            };
            ToolStrip strip = null;
            foreach (var control in pre.Controls)
            {
                if (control is ToolStrip)
                {
                    strip = (ToolStrip) control;
                    break;
                }
            }

            if (strip != null)
            {
                var btn1 = new ToolStripButton("打印机设置");
                btn1.Click += (s, e) =>
                {
                    var dlg = new PrintDialog();
                    dlg.Document = pre.Document;
                    dlg.ShowDialog();
                };
                var btn2 = new ToolStripButton("页面设置");
                btn2.Click += (s, e) =>
                {
                    var dlg = new PageSetupDialog();
                    dlg.Document = pre.Document;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        pre.PrintPreviewControl.InvalidatePreview();
                    }
                };
                strip.Items.Insert(1, btn1);
                strip.Items.Insert(2, btn2);
            }

            return pre;
        }

        private void Document_PrintPage(object sender, PrintPageEventArgs e)
        {
            var margin = e.MarginBounds;
            var w = margin.Width;
            var h = margin.Height;
            var srcW = _width;
            var rate = (double)srcW / w;
            var srcH = (int)(h * rate);
            var srcY = _preY;
            var getH = srcH;

            if (srcY + srcH > _height)
            {
                getH = _height - srcY;
                e.HasMorePages = false;
            }
            else
            {
                e.HasMorePages = true;
            }

            var bmp = new Bitmap(srcW, srcH);
            using (var gdi = Graphics.FromImage(bmp))
            {
                //gdi.DrawImage(jietu,
                //    0, 0,
                //    new Rectangle(0, srcY, bmp.Width, getH),
                //    GraphicsUnit.Pixel);

                WinApi.BitBlt(gdi.GetHdc(), 0, 0, bmp.Width, getH,
                    MBApi.wkeGetViewDC(_miniblink.MiniblinkHandle), 0, srcY,
                    (int)WinConst.SRCCOPY);
            }

            var img = Utils.GetReducedImage(w, h, bmp);
            bmp.Dispose();

            e.Graphics.DrawImage(img,
                new Rectangle(margin.X, margin.Y, img.Width, img.Height),
                new Rectangle(0, 0, img.Width, img.Height),
                GraphicsUnit.Pixel);
            img.Dispose();

            _preY += srcH;
        }

        private void Document_BeginPrint(object sender, PrintEventArgs e)
        {
            _preY = 0;
        }
    }
}
