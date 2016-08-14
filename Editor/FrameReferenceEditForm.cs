using GS_PatEditor.Editor.Panels;
using GS_PatEditor.Localization;
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
    public partial class FrameReferenceEditForm : Form
    {
        private readonly Pat.Project _Project;

        public FrameReferenceEditForm(Pat.Project proj)
        {
            InitializeComponent();
            _Project = proj;
        }

        public int OpacityValue
        {
            get
            {
                return (int)numericUpDown1.Value;
            }
            set
            {
                numericUpDown1.Value = value;
            }
        }

        internal List<FrameReferenceInfo> List
        {
            get
            {
                return listView1.Items.OfType<ListViewItem>()
                    .Select(i => (FrameReferenceInfo)i.Tag).ToList();
            }
            set
            {
                listView1.Items.Clear();
                foreach (var i in value)
                {
                    listView1.Items.Add(CreateItem(i));
                }
            }
        }

        private ListViewItem CreateItem(FrameReferenceInfo info)
        {
            return new ListViewItem(new[] { FindFrame(info.Action, info.Frame), info.Action.ActionID })
            {
                Tag = info,
                Checked = info.Visible,
            };
        }

        private string FindFrame(Pat.Action a, Pat.Frame f)
        {
            for (int i = 0; i < a.Segments.Count; ++i)
            {
                var s = a.Segments[i];
                for (int j = 0; j < s.Frames.Count; ++j)
                {
                    if (s.Frames[j] == f)
                    {
                        return String.Format(FormCodeRes.FrameReferenceEditForm_ListFormat, i, j);
                    }
                }
            }
            return FormCodeRes.FrameReferenceEditForm_ListUnknown;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                listView1.SelectedItems[0].Remove();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var info = (FrameReferenceInfo)listView1.SelectedItems[0].Tag;
                var bitmap = _Project.ImageList.GetImage(info.Frame.ImageID);
                pictureBox1.Image = bitmap;
            }
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var info = (FrameReferenceInfo)e.Item.Tag;
            info.Visible = e.Item.Checked;
        }
    }
}
