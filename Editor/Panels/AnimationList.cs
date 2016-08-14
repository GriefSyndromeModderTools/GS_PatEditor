using GS_PatEditor.Localization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor.Panels
{
    class AnimationList
    {
        private readonly Editor _Parent;
        private Control _Control;

        private readonly List<AnimationListItem> _Items = new List<AnimationListItem>();
        private AnimationListItem _SelectedItem;

        public AnimationList(Editor parent)
        {
            _Parent = parent;

            RefreshList();
            _Parent.ProjectReset += RefreshList;
        }

        public void Init(Control ctrl)
        {
            _Control = ctrl;

            ctrl.Paint += ctrl_Paint;
            ctrl.Parent.Resize += Parent_Resize;

            ctrl.Height = _Items.Sum(item => (int)item.Height) + 1;

            ctrl.Click += ctrl_Click;
            ctrl.DoubleClick += ctrl_DoubleClick;

            ctrl.FindForm().MouseWheel += _Control_MouseWheel;
            ctrl.MouseWheel += _Control_MouseWheel;
        }

        private void ctrl_DoubleClick(object sender, EventArgs e)
        {
            var pos = _Control.PointToClient(Control.MousePosition);

            //find the item
            var item = FindItem(pos);
            if (!item.CanCollapse)
            {
                return;
            }
            item.IsCollapsed = !item.IsCollapsed;

            RefreshList();
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
                point.Y = -point.Y + (e.Delta / -6);
                p.AutoScrollPosition = point;
            }
        }

        private void ctrl_Click(object sender, EventArgs e)
        {
            var pos = _Control.PointToClient(Control.MousePosition);

            //find the item
            var item = FindItem(pos);
            if (!item.CanSelect)
            {
                return;
            }

            if (_SelectedItem != null)
            {
                _SelectedItem.IsSelected = false;
            }
            _SelectedItem = item;
            if (item != null)
            {
                item.IsSelected = true;
            }
            OnSelectChange();
        }

        private void OnSelectChange()
        {
            if (SelectedChange != null)
            {
                SelectedChange();
            }

            _Control.Invalidate();
        }

        private AnimationListItem FindItem(Point pos)
        {
            var height = 0.0f;
            foreach (var a in _Items)
            {
                height += a.Height;
                if (pos.Y < height)
                {
                    return a;
                }
            }
            return null;
        }

        private void Parent_Resize(object sender, EventArgs e)
        {
            _Control.Invalidate();
        }

        private void ctrl_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            foreach (var a in _Items)
            {
                a.Render(e.Graphics, _Control.ClientSize.Width);
                e.Graphics.TranslateTransform(0, a.Height);
            }
        }

        private void RefreshList()
        {
            var lastSelectedAction = _SelectedItem == null ? null : _SelectedItem.Object;
            var unsortedItems = new List<AnimationListItem>();
            var addedCategories = new HashSet<string>();
            var lastCollapsed = _Items.Where(i => i.CanCollapse && i.IsCollapsed).Select(i => i.CategoryName).ToArray();

            _Items.Clear();

            var list = _Parent.Project.Actions;
            for (int i = 0; i < list.Count; ++i)
            {
                var a = list[i];
                unsortedItems.Add(CreateItemForAnimation(a, i));
            }

            foreach (var item in unsortedItems)
            {
                var c = item.Object.Category;
                if (c != null && c.Length > 0 && !addedCategories.Contains(c))
                {
                    addedCategories.Add(c);
                    var items = unsortedItems.Where(i => i.Object.Category == c);
                    var ci = new AnimationListItem(c, 
                        String.Format(EditorComponentRes.Animations_CategoryDescFormat, items.Count()), c);
                    _Items.Add(ci);
                    if (lastCollapsed.Contains(ci.CategoryName))
                    {
                        ci.IsCollapsed = true;
                    }
                    else
                    {
                        _Items.AddRange(items);
                    }
                }
            }
            {
                var items = unsortedItems
                    .Where(i => i.Object.Category == null || i.Object.Category.Length == 0);
                var ci = new AnimationListItem(EditorComponentRes.Animations_CategoryUncategoried,
                    String.Format(EditorComponentRes.Animations_CategoryDescFormat, items.Count()), String.Empty);
                _Items.Add(ci);
                if (lastCollapsed.Contains(ci.CategoryName))
                {
                    ci.IsCollapsed = true;
                }
                else
                {
                    _Items.AddRange(items);
                }
            }

            if (_Control != null)
            {
                _Control.Height = _Items.Sum(item => (int)item.Height) + 1;
                _Control.Invalidate();
            }

            _SelectedItem = _Items.FirstOrDefault(i => i.CanSelect && i.Object == lastSelectedAction);
            if (_SelectedItem != null)
            {
                _SelectedItem.IsSelected = true;
            }

            //TODO ensure visible

            if (SelectedChange != null)
            {
                SelectedChange();
            }
        }

        private AnimationListItem CreateItemForAnimation(Pat.Action action, int index)
        {
            var img = action.ImageID != null ?
                _Parent.Project.ImageList.GetImage(action.ImageID) :
                null;
            var frameCount = action.Segments.Sum(s => s.Frames.Count);
            var desc = String.Format(EditorComponentRes.Animations_AnimationDescFormat,
                action.Segments.Count, frameCount);
            return new AnimationListItem(img, action.ActionID, desc, index, action);
        }

        public void Activate()
        {
            if (SelectedChange != null)
            {
                SelectedChange();
            }
            //animation information might be modified, refresh
            RefreshList();
        }

        #region toolbar events

        public event Action SelectedChange;

        public bool HasSelected
        {
            get
            {
                return _SelectedItem != null;
            }
        }

        public void EditCurrent()
        {
            if (_SelectedItem != null)
            {
                _Parent.SelectedActionIndex = _SelectedItem.Index;
                _Parent.CurrentUI = EditorUI.Animation;
            }
        }

        public void RemoveCurrent()
        {
            if (_SelectedItem != null)
            {
                _Parent.Project.Actions.RemoveAt(_SelectedItem.Index);
                _SelectedItem.IsSelected = false;
                _SelectedItem = null;

                RefreshList();
            }
        }

        public void AddNew()
        {
            //find an available name
            var prefix = EditorComponentRes.Animations_NewAnimationPrefix;
            int id = 1;
            {
                var list = _Parent.Project.Actions;
                while (list.Any(a => a.ActionID == prefix + id.ToString()))
                {
                    ++id;
                }
            }

            var action = new Pat.Action()
            {
                ActionID = prefix + id.ToString(),
            };

            _Parent.Project.Actions.Add(action);

            if (_SelectedItem != null)
            {
                _SelectedItem.IsSelected = false;
                _SelectedItem = null;
            }

            RefreshList();

            _SelectedItem = _Items.Last();
            _SelectedItem.IsSelected = true;
            OnSelectChange();
        }

        public void EditProperty()
        {
            if (_SelectedItem != null)
            {
                var dialog = new AnimationPropertyFrom(_Parent.Project);
                
                var id = _SelectedItem.Index;

                var currentAnimation = _Parent.Project.Actions[id];
                dialog.AnimationID = currentAnimation.ActionID;
                dialog.Category = currentAnimation.Category;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    currentAnimation.ActionID = dialog.AnimationID;
                    currentAnimation.Category = dialog.Category;

                    RefreshList();

                    OnSelectChange();
                }
            }
        }

        #endregion
    }
}
