using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor
{
    public partial class ActionEditForm : Form
    {
        private readonly Pat.Project _Project;
        private readonly Pat.Action _Action;

        //private static readonly string
        //    NameInit = "Initialization",
        //    NameUpdate = "Update",
        //    NameStart = "SegmentStart",
        //    NameFinish = "SegmentFinish";

        public ActionEditForm(Pat.Project proj, Pat.Action action)
        {
            InitializeComponent();

            _Project = proj;
            _Action = action;

            AdjustListSize();
            RefreshList();
        }

        private void AdjustListSize()
        {
            ////make sure two list have same number of items
            //while (_Action.SegmentStartEffects.Count > _Action.SegmentFinishEffects.Count)
            //{
            //    _Action.SegmentFinishEffects.Add(new Pat.EffectList());
            //}
            //while (_Action.SegmentFinishEffects.Count > _Action.SegmentStartEffects.Count)
            //{
            //    _Action.SegmentStartEffects.Add(new Pat.EffectList());
            //}
        }

        private void AddListItem(string type, Pat.EffectList list, Pat.EffectList selected)
        {
            listBox1.Items.Add(type + "(" + list.Count + ")");
            if (list == selected)
            {
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }
        }

        private void RefreshList(Pat.EffectList selected = null)
        {
            //listBox1.Items.Clear();
            //AddListItem(NameInit, _Action.InitEffects, selected);
            //AddListItem(NameUpdate, _Action.UpdateEffects, selected);
            //
            //listBox1.Items.Add("-----");
            //
            //AdjustListSize();
            //for (int i = 0; i < _Action.SegmentStartEffects.Count; ++i)
            //{
            //    AddListItem(NameStart, _Action.SegmentStartEffects[i], selected);
            //    AddListItem(NameFinish, _Action.SegmentFinishEffects[i], selected);
            //}
            //
            //RefreshButtonEnabled();
        }

        private int ListSelectedIndex
        {
            get
            {
                return listBox1.SelectedIndex;
            }
        }

        private int ListSelectedActionIndex
        {
            get
            {
                var s = ListSelectedIndex;
                if (s <= 2)
                {
                    return -1;
                }
                return (s - 3) / 2;
            }
        }

        private bool ListSelectedActionIsStart
        {
            get
            {
                var s = ListSelectedIndex;
                if (s <= 2)
                {
                    return false;
                }
                return ((s - 3) % 2) == 0;
            }
        }

        private int GetListIndexStart(int ai)
        {
            return 3 + ai * 2;
        }

        private int GetListIndexFinish(int ai)
        {
            return 4 + ai * 2;
        }

        private Pat.EffectList GetSelectedEffectList()
        {
            //var s = ListSelectedIndex;
            //var ai = ListSelectedActionIndex;
            //switch (s)
            //{
            //    case 0:
            //        return _Action.InitEffects;
            //    case 1:
            //        return _Action.UpdateEffects;
            //    case 2:
            //        return null;
            //    default:
            //        if (ListSelectedActionIsStart)
            //        {
            //            return _Action.SegmentStartEffects[ai];
            //        }
            //        else
            //        {
            //            return _Action.SegmentFinishEffects[ai];
            //        }
            //}
            return null;
        }

        private void RefreshButtonEnabled()
        {
            if (listBox1.SelectedIndex == -1 || listBox1.SelectedIndex == 2)
            {
                button1.Enabled = true;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
            }
            else if (listBox1.SelectedIndex <= 1)
            {
                button1.Enabled = true;
                button2.Enabled = false;
                button3.Enabled = true;
                button4.Enabled = false;
                button5.Enabled = false;
            }
            else
            {
                var ai = ListSelectedActionIndex;
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = false;
                button5.Enabled = false;
                if (ListSelectedActionIndex != 0)
                {
                    button4.Enabled = true;
                }
                //if (ListSelectedActionIndex != _Action.SegmentStartEffects.Count - 1)
                //{
                //    button5.Enabled = true;
                //}
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshButtonEnabled();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //_Action.SegmentStartEffects.Add(new Pat.EffectList());
            //_Action.SegmentFinishEffects.Add(new Pat.EffectList());

            RefreshList(GetSelectedEffectList());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var ai = ListSelectedActionIndex;
            if (ai != -1)
            {
                //_Action.SegmentStartEffects.RemoveAt(ai);
                //_Action.SegmentFinishEffects.RemoveAt(ai);

                RefreshList();
            }
        }

        private void SwapSegment(int first)
        {
            //AdjustListSize();
            //
            //if (first >= 0 && first + 1 < _Action.SegmentStartEffects.Count)
            //{
            //    var s = GetSelectedEffectList();
            //
            //    var key = _Action.SegmentStartEffects[first + 1];
            //    _Action.SegmentStartEffects.RemoveAt(first + 1);
            //    _Action.SegmentStartEffects.Insert(first, key);
            //
            //    key = _Action.SegmentFinishEffects[first + 1];
            //    _Action.SegmentFinishEffects.RemoveAt(first + 1);
            //    _Action.SegmentFinishEffects.Insert(first, key);
            //
            //    RefreshList(s);
            //}
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SwapSegment(ListSelectedActionIndex - 1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SwapSegment(ListSelectedActionIndex);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var effects = GetSelectedEffectList();
            if (effects != null)
            {
                var dialog = new EffectListEditForm(_Project, effects);
                dialog.ShowDialog();
                RefreshList(effects);
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (button3.Enabled)
            {
                button3_Click(null, EventArgs.Empty);
            }
        }
    }
}
