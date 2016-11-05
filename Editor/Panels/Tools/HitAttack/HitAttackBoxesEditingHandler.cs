using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor.Panels.Tools.HitAttack
{
    enum RectPoint
    {
        None,
        LT,
        RT,
        LB,
        RB,
    }

    abstract class HitAttackBoxesEditingHandler : ClipboardHandler
    {
        private Editor _Editor;
        private Control _Control;

        public event EventFilter Filter;

        public HitAtackBoxListDataProvider BoxData { get; private set; }

        private HitAttackBoxDataProvider _SelectedSingle;
        private List<HitAttackBoxDataProvider> _SelectedMultiple = new List<HitAttackBoxDataProvider>();

        private RectPoint _EditingSingleRectPoint = RectPoint.None;
        private Point _MovingStart;
        private bool _IsMoving;

        private bool _IsRotating;
        private Point _RotateStart;
        private float _RotationBase;

        protected abstract List<Pat.Box> GetBoxListFromFrame(Pat.Frame frame);
        protected abstract FrameEditMode GetEditMode();

        public HitAttackBoxesEditingHandler(Editor editor, Control ctrl)
        {
            _Editor = editor;
            _Control = ctrl;

            Filter += editor.PreviewWindowUI.GetFilterForEditMode(GetEditMode());

            BoxData = new HitAtackBoxListDataProvider(editor, GetBoxListFromFrame);

            editor.EditModeChanged += ToolSwitched;

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

        //called when tool is closed
        public void ToolSwitched()
        {
            ClearSelected();
        }

        private void ClearSelected()
        {
            //clear previous selected
            foreach (var prev in _SelectedMultiple)
            {
                prev.IsSelected = false;
            }

            _SelectedSingle = null;
            _SelectedMultiple.Clear();
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

        #region Find Rect

        private bool IsInPointRange(Point p, int x, int y)
        {
            const float MaxDistance = 3;

            var dx = p.X - x;
            var dy = p.Y - y;
            return (dx * dx + dy * dy < MaxDistance * MaxDistance);
        }

        private bool IsInBorderRange(Point p1, Point p2, int x, int y)
        {
            if (Math.Abs(p1.X - p2.X) > Math.Abs(p1.Y - p2.Y))
            {
                return IsInBorderRange_Checked(p1.X, p2.X, p1.Y, p2.Y, x, y);
            }
            else
            {
                return IsInBorderRange_Checked(p1.Y, p2.Y, p1.X, p2.X, y, x);
            }
        }

        private bool IsInBorderRange_Checked(float x1, float x2, float y1, float y2, int x, int y)
        {
            const float MaxDistance = 3;
            if (x < x1 - MaxDistance && x < x2 - MaxDistance)
            {
                return false;
            }
            if (x > x1 + MaxDistance && x > x2 + MaxDistance)
            {
                return false;
            }
            if (Math.Abs(x1 - x2) < MaxDistance)
            {
                return IsInPointRange(new Point(x1, y1), x, y) ||
                    IsInPointRange(new Point(x2, y2), x, y);
            }
            var y0 = (x - x1) / (x2 - x1) * (y2 - y1) + y1;
            return Math.Abs(y0 - y) < MaxDistance;
        }

        private HitAttackBoxDataProvider FindBoxAt(int x, int y)
        {
            foreach (var box in BoxData.DataList)
            {
                if (IsInPointRange(box.LeftTop, x, y) ||
                    IsInPointRange(box.LeftBottom, x, y) ||
                    IsInPointRange(box.RightTop, x, y) ||
                    IsInPointRange(box.RightBottom, x, y))
                {
                    return box;
                }
            }
            return null;
        }

        private HitAttackBoxDataProvider FindBoxAtEdge(int x, int y)
        {
            foreach (var box in BoxData.DataList)
            {
                if (IsInBorderRange(box.LeftTop, box.RightTop, x, y) ||
                    IsInBorderRange(box.RightBottom, box.RightTop, x, y) ||
                    IsInBorderRange(box.RightBottom, box.LeftBottom, x, y) ||
                    IsInBorderRange(box.LeftTop, box.LeftBottom, x, y))
                {
                    return box;
                }
            }
            return null;
        }

        private RectPoint FindPointAt(HitAttackBoxDataProvider box, int x, int y)
        {
            if (IsInPointRange(box.LeftTop, x, y))
            {
                return RectPoint.LT;
            }
            if (IsInPointRange(box.LeftBottom, x, y))
            {
                return RectPoint.LB;
            }
            if (IsInPointRange(box.RightTop, x, y))
            {
                return RectPoint.RT;
            }
            if (IsInPointRange(box.RightBottom, x, y))
            {
                return RectPoint.RB;
            }
            return RectPoint.None;
        }

        #endregion
        
        private void UpdateMouseCursor(int x, int y)
        {
            if (_SelectedSingle == null && _SelectedMultiple.Count == 0)
            {
                if (FindBoxAtEdge(x, y) == null)
                {
                    //TODO check default cursor: use Cursors.Default
                    _Control.Cursor = Cursors.Arrow;
                }
                else
                {
                    _Control.Cursor = Cursors.Hand;
                }
            }
            else if (_SelectedSingle != null && _SelectedMultiple.Count == 1)
            {
                var box = FindBoxAtEdge(x, y);
                if (Control.ModifierKeys.HasFlag(Keys.Control) && box != null)
                {
                    _Control.Cursor = Cursors.Hand;
                }
                else if (box == _SelectedSingle)
                {
                    if (FindPointAt(_SelectedSingle, x, y) != RectPoint.None)
                    {
                        _Control.Cursor = Cursors.Cross;
                    }
                    else
                    {
                    _Control.Cursor = Cursors.SizeAll;
                    }
                }
                else
                {
                    _Control.Cursor = Cursors.Arrow;
                }
            }
            else
            {
                var box = FindBoxAtEdge(x, y);
                if (Control.ModifierKeys.HasFlag(Keys.Control) && box != null)
                {
                    _Control.Cursor = Cursors.Hand;
                }
                else if (box != null && box.IsSelected)
                {
                    _Control.Cursor = Cursors.SizeAll;
                }
                else
                {
                    _Control.Cursor = Cursors.Arrow;
                }
            }
        }

        private void SetSingleSelected(HitAttackBoxDataProvider box)
        {
            ClearSelected();

            _SelectedSingle = box;

            _SelectedMultiple.Clear();
            _SelectedMultiple.Add(box);

            box.IsSelected = true;
        }

        private void AppendSelected(HitAttackBoxDataProvider box)
        {
            if (box.IsSelected)
            {
                //remove
                box.IsSelected = false;
                _SelectedMultiple.Remove(box);
                _SelectedSingle = _SelectedMultiple.LastOrDefault();
            }
            else
            {
                //add
                box.IsSelected = true;
                _SelectedMultiple.Add(box);
                _SelectedSingle = box;
            }
        }

        void _Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (_EditingSingleRectPoint != RectPoint.None)
            {
                var p = new Point(e.X, e.Y);
                switch (_EditingSingleRectPoint)
                {
                    case RectPoint.LT:
                        _SelectedSingle.LeftTop = p;
                        break;
                    case RectPoint.LB:
                        _SelectedSingle.LeftBottom = p;
                        break;
                    case RectPoint.RT:
                        _SelectedSingle.RightTop = p;
                        break;
                    case RectPoint.RB:
                        _SelectedSingle.RightBottom = p;
                        break;
                    default:
                        break;
                }
                return;
            }
            if (_IsMoving)
            {
                foreach (var b in _SelectedMultiple)
                {
                    b.MovingOffset = new Point(e.X, e.Y).Relative(_MovingStart);
                }
            }
            if (_IsRotating)
            {
                var x0 = _Editor.PreviewWindowUI.PreviewMoving.TransformXSpriteToClient(0);
                var y0 = _Editor.PreviewWindowUI.PreviewMoving.TransformYSpriteToClient(0);
                _SelectedSingle.Rotation = (float)Math.Atan2(e.Y - y0, e.X - x0) -
                    (float)Math.Atan2(_RotateStart.Y - y0, _RotateStart.X - x0) + _RotationBase;
            }

            //check filter
            if (!CheckFilter())
            {
                return;
            }

            //update cursor
            UpdateMouseCursor(e.X, e.Y);
        }

        void _Control_MouseDown(object sender, MouseEventArgs e)
        {
            if (!CheckFilter() || e.Button != MouseButtons.Left)
            {
                return;
            }
            
            if (Control.ModifierKeys.HasFlag(Keys.Control))
            {
                var box = FindBoxAtEdge(e.X, e.Y);
                if (box != null)
                {
                    AppendSelected(box);
                }
                return;
            }

            if (_SelectedSingle == null && _SelectedMultiple.Count == 0)
            {
                var box = FindBoxAtEdge(e.X, e.Y);
                if (box != null)
                {
                    SetSingleSelected(box);
                }
            }
            else if (_SelectedSingle != null && _SelectedMultiple.Count == 1)
            {
                if (Control.ModifierKeys.HasFlag(Keys.Alt))
                {
                    _IsRotating = true;
                    _RotateStart = new Point(e.X, e.Y);
                    _SelectedSingle.IsEditing = true;
                    _RotationBase = _SelectedSingle.Rotation;
                    return;
                }

                var box = FindBoxAtEdge(e.X, e.Y);
                var boxpoint = FindPointAt(_SelectedSingle, e.X, e.Y);
                if (box == _SelectedSingle && boxpoint == RectPoint.None)
                {
                    _SelectedSingle.IsMoving = true;
                    _IsMoving = true;
                    _MovingStart = new Point(e.X, e.Y);
                    return;
                }
                switch (boxpoint)
                {
                    case RectPoint.None:
                        ClearSelected();
                        break;
                    default:
                        _EditingSingleRectPoint = boxpoint;
                        _SelectedSingle.IsEditing = true;
                        break;
                }
            }
            else
            {
                var box = FindBoxAtEdge(e.X, e.Y);
                if (box == null)
                {
                    ClearSelected();
                }
                else if (box != null && box.IsSelected)
                {
                    foreach (var b in _SelectedMultiple)
                    {
                        b.IsMoving = true;
                    }
                    _IsMoving = true;
                    _MovingStart = new Point(e.X, e.Y);
                }
                else
                {
                    ClearSelected();
                    //SetSingleSelected(box);
                }
            }
        }

        private void _Control_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButtons.Left))
            {
                FinishMouseEvent();
            }
        }

        private void FinishMouseEvent()
        {
            if (_EditingSingleRectPoint != RectPoint.None)
            {
                _EditingSingleRectPoint = RectPoint.None;
                _SelectedSingle.IsEditing = false;
            }
            if (_IsMoving)
            {
                _IsMoving = false;
                foreach (var b in _SelectedMultiple)
                {
                    b.IsMoving = false;
                }
            }
            if (_IsRotating)
            {
                _IsRotating = false;
                _SelectedSingle.IsEditing = false;
            }
        }

        #region clipboard

        public string DataID
        {
            get { return "GSPatEditor_HitBoxList"; }
        }

        public bool SelectedAvailable
        {
            get
            {
                return _Editor.CurrentFrame != null && _SelectedMultiple.Count > 0;
            }
        }

        public bool ClipboardDataAvailable(object data)
        {
            return data is List<Pat.Box>;
        }

        public object Copy()
        {
            FinishMouseEvent();
            var frame = _Editor.CurrentFrame;
            if (frame == null)
            {
                return null;
            }
            return _SelectedMultiple.Select(b => GetBoxListFromFrame(frame)[b.Index]).ToList();
        }

        public void Delete(bool isCut)
        {
            FinishMouseEvent();
            var frame = _Editor.CurrentFrame;
            if (frame != null)
            {
                var list = _SelectedMultiple.Select(b => b.Index).OrderBy(i => -i).ToArray();
                //remove in reversed order
                foreach (var i in list)
                {
                    GetBoxListFromFrame(frame).RemoveAt(i);
                }
            }
            ClearSelected();
            BoxData.ResetDataList();
        }

        public void Paste(object data)
        {
            FinishMouseEvent();
            var list = data as List<Pat.Box>;
            if (list == null)
            {
                return;
            }
            ClearSelected();
            var frame = _Editor.CurrentFrame;
            if (frame != null)
            {
                var boxList = GetBoxListFromFrame(frame);
                int startIndex = boxList.Count;
                boxList.AddRange(list);
                BoxData.ResetDataList();
                for (int i = startIndex; i < boxList.Count; ++i)
                {
                    AppendSelected(BoxData.DataList[i]);
                }
            }
        }

        public void New()
        {
            Paste(new List<Pat.Box>() { new Pat.Box() { X = -10, Y = -10, W = 20, H = 20 } });
        }

        public bool NewItemAvailabel
        {
            get
            {
                return _Editor.CurrentFrame != null;
            }
        }

        #endregion
    }
}
