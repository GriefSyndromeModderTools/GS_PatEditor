using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor.Panels.Tools.Physical
{
    interface IRectDataProvider
    {
        float Left { get; set; }
        float Top { get; set; }
        float Right { get; set; }
        float Bottom { get; set; }
    }

    enum RectPoint
    {
        None,
        LT,
        RT,
        LB,
        RB,
    }

    class PhysicalEditingHandler : ClipboardHandler
    {
        private const int EditMargin = 3;
        private readonly Editor _Editor;
        private readonly Control _Control;

        private PhysicalDataProvider _Rect;

        public event EventFilter Filter;

        private RectPoint _EditingPoint = RectPoint.None;

        public EditingPhysicalBox PhysicalBoxData
        {
            get { return _Rect; }
        }

        public PhysicalEditingHandler(Editor editor, Control ctrl)
        {
            _Editor = editor;
            _Control = ctrl;

            _Rect = new PhysicalDataProvider(editor);

            Filter += editor.PreviewWindowUI.GetFilterForEditMode(FrameEditMode.Physical);

            _Control.MouseMove += _Control_MouseMove;
            _Control.MouseDown += _Control_MouseDown;
            _Control.MouseUp += _Control_MouseUp;

            editor.PreviewWindowUI.ShortcutEvent += delegate(ShortcutEventType type)
            {
                if (CheckFilter())
                {
                    this.ShortcutEvent(type);
                }
            };
        }

        private void _Control_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButtons.Left))
            {
                FinishMouseEvent();
            }
        }

        private void _Control_MouseDown(object sender, MouseEventArgs e)
        {
            if (!CheckFilter())
            {
                return;
            }
            if (e.Button == MouseButtons.Left)
            {
                var p = FindMovablePoint(e.X, e.Y);
                if (p == RectPoint.None)
                {
                    return;
                }
                _EditingPoint = p;

                //start edit mode
                _Rect.IsEditing = true;
            }
        }

        private void _Control_MouseMove(object sender, MouseEventArgs e)
        {
            //process editing even if filter return false
            if (_EditingPoint != RectPoint.None)
            {
                switch (_EditingPoint)
                {
                    case RectPoint.LT:
                        _Rect.ScreenLeft = e.X;
                        _Rect.ScreenTop = e.Y;
                        break;
                    case RectPoint.RT:
                        _Rect.ScreenRight = e.X;
                        _Rect.ScreenTop = e.Y;
                        break;
                    case RectPoint.LB:
                        _Rect.ScreenLeft = e.X;
                        _Rect.ScreenBottom = e.Y;
                        break;
                    case RectPoint.RB:
                        _Rect.ScreenRight = e.X;
                        _Rect.ScreenBottom = e.Y;
                        break;
                    default:
                        break;
                }
                
                _UpdateMouseCursor(e.X, e.Y);
                return;
            }

            //then check filter for mouse cursor
            if (!CheckFilter())
            {
                return;
            }
            _UpdateMouseCursor(e.X, e.Y);
        }

        private void FinishMouseEvent()
        {
            if (_EditingPoint != RectPoint.None)
            {
                _EditingPoint = RectPoint.None;
                _Rect.IsEditing = false;
            }
        }

        private bool CheckFilter()
        {
            if (Filter == null)
            {
                return true;
            }

            bool ret = true;
            Filter(ref ret);
            return ret;
        }

        private bool CheckMargin(float diff)
        {
            return diff > -EditMargin && diff < EditMargin;
        }

        private RectPoint FindMovablePoint(int x, int y)
        {
            int result = 0;
            if (CheckMargin(x - _Rect.ScreenLeft))
            {
                result += 1;
            }
            else if (CheckMargin(x - _Rect.ScreenRight))
            {
                result += 2;
            }
            if (CheckMargin(y - _Rect.ScreenTop))
            {
                result += 4;
            }
            else if (CheckMargin(y - _Rect.ScreenBottom))
            {
                result += 8;
            }
            switch (result)
            {
                case 1 + 4: return RectPoint.LT;
                case 2 + 4: return RectPoint.RT;
                case 1 + 8: return RectPoint.LB;
                case 2 + 8: return RectPoint.RB;
                default: return RectPoint.None;
            }
        }

        private void _UpdateMouseCursor(int x, int y)
        {
            var dir = FindMovablePoint(x, y);
            switch (dir)
            {
                case RectPoint.None:
                    _Control.Cursor = Cursors.Arrow;
                    break;
                case RectPoint.LT:
                case RectPoint.RB:
                    _Control.Cursor = Cursors.SizeNWSE;
                    break;
                case RectPoint.LB:
                case RectPoint.RT:
                    _Control.Cursor = Cursors.SizeNESW;
                    break;
            }
        }

        public void UpdateMouseCursor()
        {
            if (!CheckFilter())
            {
                return;
            }
            var pos = _Control.PointToClient(Control.MousePosition);
            _UpdateMouseCursor(pos.X, pos.Y);
        }

        #region Clipboard
        public bool SelectedAvailable
        {
            get
            {
                var frame = _Editor.CurrentFrame;
                return frame != null && frame.PhysicalBox != null;
            }
        }

        public bool ClipboardDataAvailable(object data)
        {
            var frame = _Editor.CurrentFrame;
            return frame != null && data is Pat.PhysicalBox;
        }

        public object Copy()
        {
            FinishMouseEvent();
            var frame = _Editor.CurrentFrame;
            return frame != null ? frame.PhysicalBox : null;
        }

        public void Delete()
        {
            FinishMouseEvent();
            var frame = _Editor.CurrentFrame;
            if (frame != null)
            {
                frame.PhysicalBox = null;
            }
        }

        public void Paste(object data)
        {
            FinishMouseEvent();
            var frame = _Editor.CurrentFrame;
            if (data == null && !(data is Pat.PhysicalBox) || frame == null)
            {
                return;
            }
            frame.PhysicalBox = data as Pat.PhysicalBox;
        }

        public string DataID
        {
            get { return "GSPatEditor_PhysicalBox"; }
        }

        public bool NewItemAvailabel
        {
            get
            {
                var frame = _Editor.CurrentFrame;
                return frame != null && frame.PhysicalBox == null;
            }
        }

        public void New()
        {
            var frame = _Editor.CurrentFrame;
            if (frame != null)
            {
                frame.PhysicalBox = new Pat.PhysicalBox
                {
                    X = -20,
                    Y = -90,
                    W = 40,
                    H = 90,
                };
            }
        }

        #endregion
    }
}
