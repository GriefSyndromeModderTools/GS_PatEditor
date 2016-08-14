using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Editor.Panels
{
    class AnimationListItem
    {
        private const int FrameGridSize = 64;
        private Image _Image;
        private PointF _Position;
        private string _Text, _Desc;
        private int _Index;
        private Pat.Action _Obj;

        public AnimationListItem(Bitmap image, string text, string desc, int index, Pat.Action obj)
        {
            if (image != null)
            {
                float ratio = 1.0f * image.Width / image.Height;

                if (ratio > 1.0f)
                {
                    _Image = image.GetThumbnailImage(FrameGridSize, (int)(FrameGridSize / ratio), null, IntPtr.Zero);
                    _Position = new PointF(0, (FrameGridSize - (FrameGridSize / ratio)) / 2);
                }
                else
                {
                    _Image = image.GetThumbnailImage((int)(FrameGridSize * ratio), FrameGridSize, null, IntPtr.Zero);
                    _Position = new PointF((FrameGridSize - (FrameGridSize * ratio)) / 2, 0);
                }
            }

            _Text = text;
            _Desc = desc;
            _Index = index;
            _Obj = obj;

            CanSelect = true;
        }

        public AnimationListItem(string displayName, string desc, string catName)
        {
            _Text = displayName;
            _Desc = desc;
            _Index = -1;
            _Obj = null;

            CategoryName = catName;
            CanSelect = false;
        }

        public bool CanSelect { get; private set; }
        public bool CanCollapse { get { return !CanSelect; } }

        public bool IsSelected { get; set; }
        public bool IsCollapsed { get; set; }

        public float Height { get { return FrameGridSize; } }
        public int Index { get { return _Index; } }
        public Pat.Action Object { get { return _Obj; } }

        public string CategoryName { get; private set; }

        private Pen _Pen = Pens.Black;
        private Brush _BrushBackground = new SolidBrush(Color.FromArgb(200, 200, 200));
        private Brush _Brush = Brushes.Black;
        private Brush _BrushDesc = Brushes.Gray;
        private Brush _BrushBackground2 = new SolidBrush(Color.FromArgb(230, 230, 230));
        private static Font _Font = new Font(SystemFonts.DialogFont.FontFamily, FrameGridSize / 5);

        public void Render(Graphics g, float w)
        {
            var width = w + 10;

            if (IsSelected)
            {
                g.FillRectangle(_BrushBackground, 0, 0, width, FrameGridSize);
            }
            else if (_Index == -1)
            {
                g.FillRectangle(_BrushBackground2, 0, 0, width, FrameGridSize);
            }

            g.DrawLine(_Pen, 0, 0, width, 0);
            g.DrawLine(_Pen, 0, FrameGridSize, width, FrameGridSize);

            if (_Image != null)
            {
                g.DrawImage(_Image, _Position);
            }
            g.DrawString(_Text, _Font, _Brush, FrameGridSize + 10, FrameGridSize / 6);
            g.DrawString(_Desc, _Font, _BrushDesc, FrameGridSize + 10, FrameGridSize / 2);
        }
    }
}
