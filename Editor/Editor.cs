using GS_PatEditor.Editor.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor
{
    enum EditorUI
    {
        AnimationList,
        Animation,
    }

    struct FrameIndex
    {
        public readonly int Segment;
        public readonly int Frame;

        public FrameIndex(int segment, int frame)
        {
            Segment = segment;
            Frame = frame;
        }
    }

    enum FrameEditMode
    {
        None,
        Move,
        Physical,
        Hit,
        Attack,
        Point,
    }

    enum FramePreviewMode
    {
        Pause,
        Play,
    }

    class Editor : IDisposable
    {
        //ui

        public AnimationFrames AnimationFramesUI { get; private set; }
        public PreviewWindow PreviewWindowUI { get; private set; }
        public AnimationList AnimationListUI { get; private set; }

        public Editor(Pat.Project proj)
        {
            ProjectReset += ResetActionIndex;
            AnimationReset += ResetPreviewMode;
            AnimationReset += ResetEditMode;

            Project = proj;

            AnimationFramesUI = new AnimationFrames(this);
            PreviewWindowUI = new PreviewWindow(this);
            AnimationListUI = new AnimationList(this);
        }

        public void Dispose()
        {
            PreviewWindowUI.Dispose();
            PreviewWindowUI = null;
        }

        #region Main UI

        public event Action CurrentUISwitched;

        private EditorUI _CurrentUI;
        public EditorUI CurrentUI
        {
            get
            {
                return _CurrentUI;
            }
            set
            {
                _CurrentUI = value;
                if (value == EditorUI.AnimationList)
                {
                    AnimationListUI.Activate();
                }
                if (CurrentUISwitched != null)
                {
                    CurrentUISwitched();
                }
            }
        }

        #endregion

        #region Project Object Access

        public event Action ProjectReset;

        private Pat.Project _Project;
        public Pat.Project Project
        {
            get
            {
                return _Project;
            }
            set
            {
                _Project = value;
                if (ProjectReset != null)
                {
                    ProjectReset();
                }
            }
        }

        #endregion

        #region AnimationIndex

        public event Action AnimationReset;

        public Pat.Action CurrentAction { get; private set; }

        private int _SelectedActionIndex;
        public int SelectedActionIndex
        {
            get
            {
                return _SelectedActionIndex;
            }
            set
            {
                _SelectedActionIndex = value;
                if (value == -1)
                {
                    CurrentAction = null;
                }
                else
                {
                    CurrentAction = Project.Actions[value];
                }

                SelectedFrameIndex = new FrameIndex(0, 0);

                if (AnimationReset != null)
                {
                    AnimationReset();
                }
            }
        }

        private void ResetActionIndex()
        {
            SelectedActionIndex = Project.Actions.Count == 0 ? -1 : 0;
        }

        #endregion

        #region FrameIndex

        public Pat.AnimationSegment CurrentSegment { get; private set; }
        public Pat.Frame CurrentFrame { get; private set; }

        public event Action FrameReset;

        private int _SelectedFrameIndex, _SelectedSegmentIndex;
        public FrameIndex SelectedFrameIndex
        {
            get
            {
                return new FrameIndex(_SelectedSegmentIndex, _SelectedFrameIndex);
            }
            set
            {
                var segment = value.Segment;
                var frame = value.Frame;

                if (CurrentAction == null)
                {
                    ResetFrame(null, null);
                    return;
                }

                if (segment == -1 || frame == -1)
                {
                    ResetFrame(null, null);
                    return;
                }

                if (segment >= CurrentAction.Segments.Count)
                {
                    if (CurrentAction.Segments.Count == 0)
                    {
                        _SelectedSegmentIndex = -1;
                        _SelectedFrameIndex = -1;
                        ResetFrame(null, null);
                        return;
                    }
                    else
                    {
                        segment = 0;
                    }
                }

                _SelectedSegmentIndex = segment;
                var seg = CurrentAction.Segments[segment];

                if (frame >= seg.Frames.Count)
                {
                    if (seg.Frames.Count == 0)
                    {
                        _SelectedFrameIndex = -1;
                        ResetFrame(seg, null);
                        return;
                    }
                    else
                    {
                        frame = 0;
                    }
                }
                _SelectedFrameIndex = frame;
                ResetFrame(seg, seg.Frames[frame]);
            }
        }

        private void ResetFrame(Pat.AnimationSegment segment, Pat.Frame frame)
        {
            CurrentSegment = segment;
            CurrentFrame = frame;

            if (FrameReset != null)
            {
                FrameReset();
            }
        }

        #endregion

        #region Show Forms

        public void ShowActionEditForm()
        {
            if (CurrentAction != null)
            {
                //if (CurrentAction.ContainsLowLevelEffects)
                //{
                //    //advanced mode
                //    if (CurrentAction.Behaviors.Count > 0)
                //    {
                //        MessageBox.Show("Advanced mode. Behaviors is neglated. " +
                //            "Remove all the effects to edit behaviors.");
                //    }
                //    var dialog = new ActionEditForm(Project, CurrentAction);
                //    dialog.ShowDialog();
                //}
                //else
                //{
                //    //normal mode
                //    var dialog = new ActionBehaviorEditForm(Project, CurrentAction);
                //    dialog.ShowDialog();
                //}
                var dialog = new ActionBehaviorEditForm(Project, CurrentAction);
                dialog.ShowDialog();
            }
        }

        #endregion

        #region Preview Visibles

        public bool AxisVisible = true;
        public bool PhysicalBoxVisible = true;
        public bool HitBoxVisible = true;
        public bool AttackBoxVisible = true;
        public bool PointVisible = true;
        public bool BorderVisible = true;

        #endregion

        #region EditMode

        private FrameEditMode _EditMode;
        public FrameEditMode EditMode
        {
            get
            {
                return _EditMode;
            }
            set
            {
                if (_EditMode == value)
                {
                    return;
                }

                _EditMode = value;

                if (EditModeChanged != null)
                {
                    EditModeChanged();
                }
            }
        }

        public event Action EditModeChanged;

        private void ResetEditMode()
        {
            EditMode = FrameEditMode.None;
        }

        #endregion

        #region PreviewMode

        private FramePreviewMode _PreviewMode;
        public FramePreviewMode PreviewMode
        {
            get
            {
                return _PreviewMode;
            }
            set
            {
                if (_PreviewMode == value)
                {
                    return;
                }

                _PreviewMode = value;

                if (PreviewModeChanged != null)
                {
                    PreviewModeChanged();
                }

                EditMode = FrameEditMode.None;
            }
        }

        public event Action PreviewModeChanged;

        private void ResetPreviewMode()
        {
            PreviewMode = FramePreviewMode.Pause;
        }

        #endregion
    }
}
