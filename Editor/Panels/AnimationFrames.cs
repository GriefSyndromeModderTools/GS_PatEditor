using GS_PatEditor.Localization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor.Panels
{
    class AnimationFrames : ClipboardHandler
    {
        #region Frame Grid
        private const int FrameGridSize = 64;

        [Flags]
        enum KeyFrameFlags
        {
            IsKeyFrame = 1,
            HasDamage = 2,
            SkillCancellable = 4,
            JumpCancellable = 8,
        }

        abstract class AbstractGrid
        {
            public abstract void Render(Graphics g);
            public virtual int Width { get { return FrameGridSize; } }
        }
        class EmptyGrid : AbstractGrid
        {
            private Pen pen = new Pen(Color.Black);

            public override void Render(Graphics g)
            {
                g.DrawRectangle(pen, 0, 0, FrameGridSize, FrameGridSize);
            }
        }
        abstract class ImageGrid : AbstractGrid
        {
            private Image _Image;
            private PointF _Position;
            private string _Index1;
            private string _Index2;
            private Brush _TextBrush = new SolidBrush(Color.Black);

            public ImageGrid(Bitmap image, int index1, int? index2)
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

                _Index1 = index1.ToString();
                _Index2 = index2.HasValue ? index2.ToString() : "";
            }

            protected void DrawImage(Graphics g)
            {
                if (_Image != null)
                {
                    g.DrawImage(_Image, _Position);
                }
                if (_Index1 != null)
                {
                    g.DrawString(_Index1, SystemFonts.DefaultFont, _TextBrush,
                        new RectangleF(0, 0, FrameGridSize / 2, FrameGridSize / 2));
                }
                if (_Index2 != null)
                {
                    g.DrawString(_Index2, SystemFonts.DefaultFont, _TextBrush,
                        new RectangleF(FrameGridSize / 2, 0, FrameGridSize / 2, FrameGridSize / 2),
                        new StringFormat() { Alignment = StringAlignment.Far });
                }
            }
        }
        class KeyFrameGrid : ImageGrid
        {
            private Pen pen = new Pen(Color.Black);
            private Brush brush = new SolidBrush(Color.FromArgb(215, 215, 215));
            private Brush brushSelected = new SolidBrush(Color.FromArgb(120, 120, 120));
            private Brush brushKey = new SolidBrush(Color.FromArgb(100, 140, 255));
            private Brush brushDamage = new SolidBrush(Color.FromArgb(222, 36, 18));
            private Brush brushSkillC = new SolidBrush(Color.FromArgb(90, 250, 4));
            private Brush brushJumpC = new SolidBrush(Color.FromArgb(255, 240, 100));
            public readonly int Segment, Frame;
            public readonly KeyFrameFlags Flags;
            public readonly Pat.Frame FrameObject;

            public KeyFrameGrid(Bitmap bitmap, int index, int segIndex, int frameIndex, KeyFrameFlags flags, Pat.Frame frameObj)
                : base(bitmap, index, frameIndex == 0 ? segIndex : (int?)null)
            {
                Segment = segIndex;
                Frame = frameIndex;
                Flags = flags;
                FrameObject = frameObj;

                IsFolded = true;
            }

            public override void Render(Graphics g)
            {
                g.FillRectangle(IsSelected ? brushSelected : brush, 0, 0, FrameGridSize, FrameGridSize);
                g.DrawRectangle(pen, 0, 0, FrameGridSize, FrameGridSize);
                DrawImage(g);
                if (Flags.HasFlag(KeyFrameFlags.IsKeyFrame))
                {
                    g.FillEllipse(brushKey,
                        FrameGridSize * 0.02f, FrameGridSize * 0.25f,
                        FrameGridSize * 0.10f, FrameGridSize * 0.10f);
                }
                if (Flags.HasFlag(KeyFrameFlags.HasDamage))
                {
                    g.FillEllipse(brushDamage,
                        FrameGridSize * 0.02f, FrameGridSize * 0.40f,
                        FrameGridSize * 0.10f, FrameGridSize * 0.10f);
                }
                if (Flags.HasFlag(KeyFrameFlags.JumpCancellable))
                {
                    g.FillEllipse(brushJumpC,
                        FrameGridSize * 0.02f, FrameGridSize * 0.55f,
                        FrameGridSize * 0.10f, FrameGridSize * 0.10f);
                }
                if (Flags.HasFlag(KeyFrameFlags.SkillCancellable))
                {
                    g.FillEllipse(brushSkillC,
                        FrameGridSize * 0.02f, FrameGridSize * 0.70f,
                        FrameGridSize * 0.10f, FrameGridSize * 0.10f);
                }
            }

            public bool IsSelected { get; set; }
            public bool IsFolded { get; set; }
        }
        class NormalFrameGrid : ImageGrid
        {
            private Pen pen = new Pen(Color.Black);
            private Brush brush = new SolidBrush(Color.FromArgb(230, 230, 230));
            public readonly KeyFrameGrid KeyFrame;

            public NormalFrameGrid(KeyFrameGrid key, Bitmap bitmap, int index)
                : base(bitmap, index, null)
            {
                KeyFrame = key;
            }

            public override void Render(Graphics g)
            {
                if (Visible)
                {
                    g.FillRectangle(brush, 0, 0, FrameGridSize, FrameGridSize);
                    g.DrawRectangle(pen, 0, 0, FrameGridSize, FrameGridSize);
                    DrawImage(g);
                }
                else
                {
                    g.FillRectangle(brush, 1, 0, 2, FrameGridSize);
                    g.DrawLine(pen, 0, 0, 3, 0);
                    g.DrawLine(pen, 0, FrameGridSize, 3, FrameGridSize);
                }
            }

            public override int Width
            {
                get
                {
                    return Visible ? FrameGridSize : 3;
                }
            }

            public bool Visible { get { return !KeyFrame.IsFolded; } }
        }
        #endregion

        private readonly Editor _Parent;
        private Control _Control;
        private readonly List<AbstractGrid> _GridList = new List<AbstractGrid>();
        private KeyFrameGrid _LastSelected;

        public AnimationFrames(Editor parent)
        {
            _Parent = parent;

            parent.AnimationReset += OnAnimationReset;

            RefreshList();
        }

        public void Init(Control ctrl)
        {
            this._Control = ctrl;

            ctrl.Paint += ctrl_Paint;
            ctrl.Click += ctrl_Click;
            ctrl.DoubleClick += ctrl_DoubleClick;

            UpdateControlWidth();

            ctrl.FindForm().MouseWheel += _Control_MouseWheel;
            ctrl.MouseWheel += _Control_MouseWheel;
        }

        private void _Control_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!_Control.IsMouseInRange())
            {
                return;
            }
            ScrollableControl p = _Control.Parent as ScrollableControl;
            if (p != null)
            {
                var point = p.AutoScrollPosition;
                point.X = -point.X + (e.Delta / -6);
                p.AutoScrollPosition = point;
            }
        }

        void ctrl_DoubleClick(object sender, EventArgs e)
        {
            Point pos = _Control.PointToClient(Control.MousePosition);
            var grid = GetGridFromPoint(pos.X);
            if (grid == null)
            {
                return;
            }

            if (grid is KeyFrameGrid)
            {
                var keyGrid = (KeyFrameGrid)grid;
                keyGrid.IsFolded = !keyGrid.IsFolded;
                UpdateControlWidth();
                _Control.Invalidate();
            }
            else if (grid is NormalFrameGrid)
            {
                var normalGrid = (NormalFrameGrid)grid;
                normalGrid.KeyFrame.IsFolded = !normalGrid.KeyFrame.IsFolded;
                UpdateControlWidth();
                _Control.Invalidate();
            }
        }

        private void UpdateControlWidth()
        {
            if (_Control != null)
            {
                _Control.Width = _GridList.Sum(g => g.Width) + FrameGridSize * 0 + 1;
            }
        }

        private void ctrl_Click(object sender, EventArgs e)
        {
            Point pos = _Control.PointToClient(Control.MousePosition);
            var grid = GetGridFromPoint(pos.X);
            if (grid == null)
            {
                return;
            }

            if (grid is KeyFrameGrid)
            {
                var keyGrid = (KeyFrameGrid)grid;
                SelectKeyGrid(keyGrid);
            }
            else if (grid is NormalFrameGrid)
            {
                var normalGrid = (NormalFrameGrid)grid;
                SelectKeyGrid(normalGrid.KeyFrame);
            }
            else if (grid is EmptyGrid)
            {
                SelectKeyGrid(null);
            }
        }

        private void SelectKeyGrid(KeyFrameGrid grid)
        {
            if (_LastSelected == grid)
            {
                return;
            }
            if (_LastSelected != null)
            {
                _LastSelected.IsSelected = false;
            }
            if (grid != null)
            {
                grid.IsSelected = true;
                _Parent.SelectedFrameIndex = new FrameIndex(grid.Segment, grid.Frame);
            }
            else
            {
                //clear selected
                _Parent.SelectedFrameIndex = new FrameIndex(-1, -1);
            }
            _LastSelected = grid;

            if (_Control != null)
            {
                _Control.Invalidate();
            }
        }

        public void SelectLastKeyGrid()
        {
            if (_GridList.Count == 0)
            {
                return;
            }
            var list = GetKeyFrames();
            var index = GetKeyFrameIndex(list, _LastSelected);
            index = (index - 1 + list.Count) % list.Count;
            SelectKeyGrid(list[index]);
        }

        public void SelectNextKeyGrid()
        {
            if (_GridList.Count == 0)
            {
                return;
            }
            var list = GetKeyFrames();
            var index = GetKeyFrameIndex(list, _LastSelected);
            index = (index + 1) % list.Count;
            SelectKeyGrid(list[index]);
        }

        private List<KeyFrameGrid> _CachedKeyFrame = new List<KeyFrameGrid>();
        private List<KeyFrameGrid> GetKeyFrames()
        {
            _CachedKeyFrame.Clear();
            foreach (var f in _GridList)
            {
                if (f is KeyFrameGrid)
                {
                    _CachedKeyFrame.Add((KeyFrameGrid)f);
                }
            }
            _CachedKeyFrame.Add(null);
            return _CachedKeyFrame;
        }

        private int GetKeyFrameIndex(List<KeyFrameGrid> list, KeyFrameGrid grid)
        {
            var index = list.FindIndex(g => g == grid);
            if (index == -1)
            {
                return list.Count - 1;
            }
            return index;
        }

        private AbstractGrid GetGridFromPoint(int x)
        {
            int sum = 0;
            foreach (var g in _GridList)
            {
                sum += g.Width;
                if (sum > x)
                {
                    return g;
                }
            }
            return null;
        }

        private void ctrl_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            foreach (var g in _GridList)
            {
                g.Render(e.Graphics);
                e.Graphics.TranslateTransform(g.Width, 0);
                e.Graphics.DrawLine(Pens.Black, 0, 0, 0, FrameGridSize);
            }
        }

        private void OnAnimationReset()
        {
            _LastSelected = null;
            RefreshList();
        }

        private void RefreshList()
        {
            //save the list
            var lastList = _GridList
                .Where(g => g is KeyFrameGrid)
                .Cast<KeyFrameGrid>().ToArray();

            _GridList.Clear();

            var data = _Parent.CurrentAction;
            if (data == null)
            {
                UpdateControlWidth();
                SelectKeyGrid(null);
                return;
            }

            var imageList = _Parent.Project.ImageList;
            KeyFrameGrid newSelected = null;
            for (int i = 0; i < data.Segments.Count; ++i)
            {
                var seg = data.Segments[i];

                //calculate cancellable frame
                int cancellableJump = -1, cancellableSkill = -1;
                if (seg.JumpCancellable != null)
                {
                    int sum = 0;
                    for (int fi = 0; fi < seg.Frames.Count; ++fi)
                    {
                        if (sum >= seg.JumpCancellable.StartFrom)
                        {
                            cancellableJump = fi;
                            break;
                        }
                        sum += seg.Frames[fi].Duration;
                    }
                }
                if (seg.SkillCancellable != null)
                {
                    int sum = 0;
                    for (int fi = 0; fi < seg.Frames.Count; ++fi)
                    {
                        if (sum >= seg.SkillCancellable.StartFrom)
                        {
                            cancellableSkill = fi;
                            break;
                        }
                        sum += seg.Frames[fi].Duration;
                    }
                }
                for (int j = 0; j < seg.Frames.Count; ++j)
                {
                    var frame = seg.Frames[j];
                    var image = frame.ImageID == null ? null : imageList.GetImage(frame.ImageID);

                    //flags
                    KeyFrameFlags flags = 0;
                    if (j == 0)
                    {
                        flags |= KeyFrameFlags.IsKeyFrame;
                    }
                    if (j == 0 && seg.Damage != null)
                    {
                        flags |= KeyFrameFlags.HasDamage;
                    }
                    if (j == cancellableJump)
                    {
                        flags |= KeyFrameFlags.JumpCancellable;
                    }
                    if (j == cancellableSkill)
                    {
                        flags |= KeyFrameFlags.SkillCancellable;
                    }

                    //create
                    var keyFrame = new KeyFrameGrid(image, _GridList.Count, i, j, flags, frame);

                    //restore status
                    //selected
                    if (_LastSelected != null && frame == _LastSelected.FrameObject)
                    {
                        keyFrame.IsSelected = true;
                        newSelected = keyFrame;
                    }
                    var lastItem = lastList.Where(g => g.FrameObject == frame).FirstOrDefault();
                    if (lastItem != null && !lastItem.IsFolded)
                    {
                        keyFrame.IsFolded = false;
                    }

                    _GridList.Add(keyFrame);
                    for (int k = 0; k < frame.Duration - 1; ++k)
                    {
                        _GridList.Add(new NormalFrameGrid(keyFrame, image, _GridList.Count));
                    }
                }
            }

            _GridList.Add(new EmptyGrid());

            UpdateControlWidth();

            if (_GridList.Count > 0 && _GridList[0] is KeyFrameGrid && newSelected == null)
            {
                newSelected = (KeyFrameGrid)_GridList[0];
            }
            SelectKeyGrid(newSelected);
        }

        public void CollapseAll()
        {
            foreach (var g in _GridList)
            {
                if (g is KeyFrameGrid)
                {
                    ((KeyFrameGrid)g).IsFolded = true;
                }
            }
            UpdateControlWidth();
            if (_Control != null)
            {
                _Control.Invalidate();
            }
        }

        public void ExpandAll()
        {
            foreach (var g in _GridList)
            {
                if (g is KeyFrameGrid)
                {
                    ((KeyFrameGrid)g).IsFolded = false;
                }
            }
            UpdateControlWidth();
            if (_Control != null)
            {
                _Control.Invalidate();
            }
        }

        #region ui event

        public void SetCurrentToKeyFrame()
        {
            if (_LastSelected != null)
            {
                var grid = (KeyFrameGrid)_LastSelected;
                var segmentIndex = grid.Segment;
                var frameIndex = grid.Frame;
                if (frameIndex == 0)
                {
                    //already a key frame
                    return;
                }

                var action = _Parent.CurrentAction;
                if (action == null)
                {
                    return;
                }

                //split a segment
                var oldSegment = action.Segments[segmentIndex];
                var newSegment = new Pat.AnimationSegment()
                {
                    IsLoop = oldSegment.IsLoop,
                    Frames = oldSegment.Frames.Skip(frameIndex).ToList(),
                };
                oldSegment.Frames = oldSegment.Frames.Take(frameIndex).ToList();

                //TODO check cancellable data in oldSegment

                action.Segments.Insert(segmentIndex + 1, newSegment);

                RefreshList();
            }
        }

        public void SetCurrentToNormalFrame()
        {
            if (_LastSelected != null)
            {
                var grid = (KeyFrameGrid)_LastSelected;
                var segmentIndex = grid.Segment;
                if (grid.Frame != 0)
                {
                    //not a key frame
                    return;
                }
                if (segmentIndex == 0)
                {
                    MessageBox.Show(EditorComponentRes.Frames_FirstMustKey,
                        EditorFormCodeRes.MsgBoxTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                var action = _Parent.CurrentAction;
                if (action == null)
                {
                    return;
                }

                //split a segment
                var lastSegment = action.Segments[segmentIndex - 1];
                var theSegment = action.Segments[segmentIndex];
                lastSegment.Frames.AddRange(theSegment.Frames);

                //TODO check cancellable data in oldSegment

                action.Segments.RemoveAt(segmentIndex);

                RefreshList();
            }
        }

        public void SwitchCurrentLoop()
        {
            if (_LastSelected != null)
            {
                var grid = (KeyFrameGrid)_LastSelected;
                var segmentIndex = grid.Segment;
                if (grid.Frame != 0)
                {
                    //not a key frame
                    return;
                }

                var action = _Parent.CurrentAction;
                if (action == null)
                {
                    return;
                }

                //split a segment
                var theSegment = action.Segments[segmentIndex];
                theSegment.IsLoop = !theSegment.IsLoop;

                RefreshList();
            }
        }

        public void ShowEditFrameForm()
        {
            if (_LastSelected != null)
            {
                var grid = (KeyFrameGrid)_LastSelected;

                var action = _Parent.CurrentAction;

                if (action == null)
                {
                    return;
                }

                var frame = action.Segments[grid.Segment].Frames[grid.Frame];

                var dialog = new FrameEditForm()
                {
                    UseImage = frame.ImageID == action.ImageID,
                    FrameCount = frame.Duration,
                    SetDuationForAllEnabled = grid.Frame == 0,
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (dialog.SetDurationForAll)
                    {
                        foreach (var f in action.Segments[grid.Segment].Frames)
                        {
                            f.Duration = dialog.FrameCount;
                        }
                    }
                    else
                    {
                        frame.Duration = dialog.FrameCount;
                    }
                    if (frame.ImageID == action.ImageID && !dialog.UseImage)
                    {
                        action.ImageID = null;
                    }
                    else if (dialog.UseImage)
                    {
                        action.ImageID = frame.ImageID;
                    }
                }

                RefreshList();
            }
        }

        public void ShowSelectImageForm()
        {
            if (_LastSelected != null)
            {
                var grid = (KeyFrameGrid)_LastSelected;

                var action = _Parent.CurrentAction;
                if (action == null)
                {
                    return;
                }
                
                var frame = action.Segments[grid.Segment].Frames[grid.Frame];

                var dialog = new ImageSelectForm(_Parent.Project)
                {
                    SelectedImage = frame.ImageID,
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    frame.ImageID = dialog.SelectedImage;
                    RefreshList();
                }
            }
            else
            {
                var dialog = new ImageSelectForm(_Parent.Project)
                {
                    SelectedImage = null,
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    //create a new frame
                    var action = _Parent.CurrentAction;
                    if (action == null)
                    {
                        return;
                    }

                    if (action.Segments.Count == 0)
                    {
                        action.Segments.Add(new Pat.AnimationSegment()
                        {
                            Frames = new List<Pat.Frame>(),
                        });
                    }
                    var segment = action.Segments.Last();

                    int duration = 1;
                    if (segment.Frames.Count > 0)
                    {
                        duration = segment.Frames.Last().Duration;
                    }

                    var frame = new Pat.Frame()
                    {
                        Duration = duration,
                        ImageID = dialog.SelectedImage,
                        AttackBoxes = new List<Pat.Box>(),
                        HitBoxes = new List<Pat.Box>(),
                        Points = new List<Pat.FramePoint>(),
                        ScaleX = 100,
                        ScaleY = 100,
                    };
                    segment.Frames.Add(frame);

                    //ensure after refreshing, the new frame is selected
                    _LastSelected = new KeyFrameGrid(null, 0, 0, 0, 0, frame);
                    
                    RefreshList();
                }

            }
        }

        public void InsertNewFrameBefore()
        {
            if (_LastSelected != null)
            {
                var grid = (KeyFrameGrid)_LastSelected;
                var segmentIndex = grid.Segment;
                var frameIndex = grid.Frame;

                var action = _Parent.CurrentAction;
                if (action == null)
                {
                    return;
                }

                var frame = new Pat.Frame()
                {
                    Duration = action.Segments[segmentIndex].Frames[frameIndex].Duration,
                    ImageID = null,
                    AttackBoxes = new List<Pat.Box>(),
                    HitBoxes = new List<Pat.Box>(),
                    Points = new List<Pat.FramePoint>(),
                    ScaleX = 100,
                    ScaleY = 100,
                };

                if (frameIndex == 0 && segmentIndex > 0)
                {
                    action.Segments[segmentIndex - 1].Frames.Add(frame);
                }
                else
                {
                    action.Segments[segmentIndex].Frames.Insert(frameIndex, frame);
                }

                RefreshList();
            }
        }

        public void ShowEditDamageForm()
        {
            if (_LastSelected != null)
            {
                var grid = (KeyFrameGrid)_LastSelected;
                var segmentIndex = grid.Segment;
                var frameIndex = grid.Frame;

                var action = _Parent.CurrentAction;
                if (action == null)
                {
                    return;
                }

                if (frameIndex != 0)
                {
                    return;
                }

                var damage = action.Segments[segmentIndex].Damage;
                var dialog = new DamageEditForm(damage);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    action.Segments[segmentIndex].Damage = dialog.GetData();
                }

                RefreshList();
            }
        }

        #endregion

        #region clipboard

        public string DataID
        {
            get { return "GSPatEditor_Frame"; }
        }

        public bool SelectedAvailable
        {
            get
            {
                return _LastSelected != null;
            }
        }

        public bool NewItemAvailabel
        {
            get
            {
                //can not insert (only paste is supported) at the beginning
                return _Parent.CurrentAction != null;
            }
        }

        public bool ClipboardDataAvailable(object data)
        {
            return data is Pat.Frame;
        }

        public void New()
        {
            //this method is not used
            throw new NotImplementedException();
        }

        public object Copy()
        {
            return _LastSelected.FrameObject;
        }

        public void Delete()
        {
            if (_LastSelected == null)
            {
                return;
            }

            var action = _Parent.CurrentAction;
            if (action == null)
            {
                return;
            }

            if (action.Segments[_LastSelected.Segment].Frames.Count == 1)
            {
                if (MessageBox.Show(EditorComponentRes.Frames_ConfirmRemoveLast,
                    EditorFormCodeRes.MsgBoxTitle,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return;
                }
                action.Segments.RemoveAt(_LastSelected.Segment);
            }
            else
            {
                if (MessageBox.Show(EditorComponentRes.Frames_ConfirmRemove,
                    EditorFormCodeRes.MsgBoxTitle,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return;
                }
                action.Segments[_LastSelected.Segment].Frames.RemoveAt(_LastSelected.Frame);
            }

            RefreshList();
        }

        public void Paste(object data)
        {
            var fdata = data as Pat.Frame;
            if (fdata == null)
            {
                return;
            }

            var action = _Parent.CurrentAction;
            if (action == null)
            {
                return;
            }

            if (_LastSelected == null)
            {
                if (action.Segments.Count == 0)
                {
                    action.Segments.Add(new Pat.AnimationSegment()
                    {
                        Frames = new List<Pat.Frame>(),
                    });
                }
                action.Segments[action.Segments.Count - 1].Frames.Add(fdata);
            }
            else
            {
                if (_LastSelected.Frame == 0)
                {
                    if (_LastSelected.Segment == 0)
                    {
                        action.Segments[_LastSelected.Segment].Frames.Insert(0, fdata);
                    }
                    else
                    {
                        if (MessageBox.Show(EditorComponentRes.Frames_PasteLastSegOrThis,
                            EditorFormCodeRes.MsgBoxTitle,
                            MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            action.Segments[_LastSelected.Segment].Frames.Insert(0, fdata);
                        }
                        else
                        {
                            action.Segments[_LastSelected.Segment - 1].Frames.Add(fdata);
                        }
                    }
                }
                else
                {
                    action.Segments[_LastSelected.Segment].Frames.Insert(_LastSelected.Frame, fdata);
                }
            }

            RefreshList();
        }

        #endregion

        #region cancel level access

        public bool CancellableEnabled
        {
            get
            {
                return _Parent.CurrentAction != null && _LastSelected != null;
            }
        }

        public int CancelLevel
        {
            get
            {
                var action = _Parent.CurrentAction;
                if (action == null || _LastSelected == null)
                {
                    return -1;
                }
                var grid = _LastSelected;
                if (grid.Frame != 0)
                {
                    return -1;
                }

                var seg = grid.Segment;
                var ret = (int)action.Segments[seg].CancelLevel - 1;
                
                if (ret == -1)
                {
                    return 0;
                }
                return ret;
            }
            set
            {
                var action = _Parent.CurrentAction;
                if (action == null || _LastSelected == null)
                {
                    return;
                }
                var grid = _LastSelected;
                if (grid.Frame != 0)
                {
                    return;
                }

                var seg = grid.Segment;
                if (value == -1)
                {
                    return;
                }
                if (action.Segments.Count > 1 &&
                    MessageBox.Show(EditorComponentRes.Frame_SetCancelLevelForAll,
                    EditorFormCodeRes.MsgBoxTitle,
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var c = (Pat.CancelLevel)value + 1;
                    foreach (var s in action.Segments)
                    {
                        s.CancelLevel = c;
                    }
                }
                else
                {
                    action.Segments[seg].CancelLevel = (Pat.CancelLevel)value + 1;
                }
            }
        }

        public bool SkillCancellable
        {
            get
            {
                if (!CancellableEnabled)
                {
                    return false;
                }

                return _LastSelected.Flags.HasFlag(KeyFrameFlags.SkillCancellable);
            }
            set
            {
                if (!CancellableEnabled)
                {
                    return;
                }

                var segment = _Parent.CurrentAction.Segments[_LastSelected.Segment];
                var time = segment.Frames.Take(_LastSelected.Frame)
                            .Sum(f => f.Duration);
                if (value)
                {
                    //set to this frame
                    segment.SkillCancellable = new Pat.AnimationCancellableInfo
                    {
                        StartFrom = time,
                    };
                }
                else if (segment.SkillCancellable != null && segment.SkillCancellable.StartFrom == time)
                {
                    //remove cancellable
                    segment.SkillCancellable = null;
                }

                RefreshList();
            }
        }

        public bool JumpCancellable
        {
            get
            {
                if (!CancellableEnabled)
                {
                    return false;
                }

                return _LastSelected.Flags.HasFlag(KeyFrameFlags.JumpCancellable);
            }
            set
            {
                if (!CancellableEnabled)
                {
                    return;
                }

                var segment = _Parent.CurrentAction.Segments[_LastSelected.Segment];
                var time = segment.Frames.Take(_LastSelected.Frame)
                            .Sum(f => f.Duration);
                if (value)
                {
                    //set to this frame
                    segment.JumpCancellable = new Pat.AnimationCancellableInfo
                    {
                        StartFrom = time,
                    };
                }
                else if (segment.JumpCancellable != null && segment.JumpCancellable.StartFrom == time)
                {
                    //remove cancellable
                    segment.JumpCancellable = null;
                }

                RefreshList();
            }
        }

        #endregion
    }
}
