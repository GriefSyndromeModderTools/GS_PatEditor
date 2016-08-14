using GS_PatEditor.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor
{
    public partial class ImportPatAnimationForm : Form
    {
        private readonly Pat.Project _Project;

        private GSPat.GSPatFile _GSPatFile;
        private string _Path;
        private string _Palette;

        public ImportPatAnimationForm(Pat.Project proj)
        {
            InitializeComponent();

            _Project = proj;
        }

        public List<Pat.AnimationSegment> ImportedSegments { get; private set; }

        private GSPat.ImageManager _Images;

        private void CancelDialog()
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void ImportPatAnimationForm_Shown(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                CancelDialog();
                return;
            }

            var file = openFileDialog1.FileName;
            var path = Path.GetDirectoryName(file);
            var palFile = Path.Combine(path, "palette000.pal");

            if (!File.Exists(palFile))
            {
                if (MessageBox.Show(FormCodeRes.ImportPatAnimationForm_PaletteNotFound,
                    FormCodeRes.ImportPatAnimationForm_MsgBoxTitle,
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.OK)
                {
                    if (openFileDialog2.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        CancelDialog();
                        return;
                    }
                    palFile = openFileDialog2.FileName;
                }
                else
                {
                    palFile = null;
                }
            }

            //create objects
            GSPat.GSPatFile gspat;
            using (var s = File.OpenRead(file))
            {
                gspat = GSPat.GSPatReader.ReadFromStream(s);
            }
            var pal = palFile != null ? Images.CV2Palette.ReadPaletteFile(palFile) : null;

            //create image manager
            _Images = new GSPat.ImageManager(gspat, path, pal);

            //refresh list
            var lastIndex = -1;
            var segmentIndex = 0;
            foreach (var animation in gspat.Animations)
            {
                ++segmentIndex;
                if (animation.AnimationID == -1)
                {
                    //can't import clone animation
                    lastIndex = -1;
                    continue;
                }
                else if (animation.AnimationID == -2)
                {
                    AddFollowerAnimation(animation, lastIndex, segmentIndex);
                }
                else
                {
                    AddAnimation(animation);
                    lastIndex = animation.AnimationID;
                    segmentIndex = 0;
                }
            }

            _GSPatFile = gspat;
            _Path = path;
            _Palette = palFile;
        }

        private void AddFollowerAnimation(GSPat.Animation a, int id, int s)
        {
            var text = String.Format(FormCodeRes.ImportPatAnimationForm_ListFormat, id, s);
            var item = new ListViewItem(text);
            item.Tag = a;
            listView1.Items.Add(item);
        }

        private void AddAnimation(GSPat.Animation a)
        {
            var text = String.Format(FormCodeRes.ImportPatAnimationForm_ListFormat, a.AnimationID);
            var item = new ListViewItem(text);
            item.Tag = a;
            listView1.Items.Add(item);
        }

        private void ImportPatAnimationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_Images != null)
            {
                _Images.Dispose();
                _Images = null;
            }
            timer1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var item = listView1.SelectedItems[0];
                listView2.Items.Add(new ListViewItem(item.Text) { Tag = item.Tag });
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                listView2.Items.Remove(listView2.SelectedItems[0]);
            }
        }

        private GSPat.Animation _SelectedAnimation;
        private int _CurrentFrameIndex;
        private int _FrameCounter;
        private int _OffsetX, _OffsetY;

        private Point _Position;
        private Bitmap _Image;

        private void SetSelectedAnimation(GSPat.Animation animation)
        {
            _SelectedAnimation = animation;
            _CurrentFrameIndex = 0;
            _FrameCounter = 0;
            _Images.Switch();
            _Image = null;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_SelectedAnimation != null && _SelectedAnimation.Frames.Count > 0)
            {
                if (_CurrentFrameIndex < 0 || _CurrentFrameIndex >= _SelectedAnimation.Frames.Count)
                {
                    if (!_SelectedAnimation.IsLoop)
                    {
                        return;
                    }

                    _CurrentFrameIndex = 0;
                    _FrameCounter = 0;
                }

                var frame = _SelectedAnimation.Frames[_CurrentFrameIndex];

                _Image = _Images.GetBitmap(frame);
                _Position = new Point(pictureBox1.ClientSize.Width / 2 + _OffsetX - frame.OriginX,
                    pictureBox1.ClientSize.Height / 2 + _OffsetY - frame.OriginY);

                ++_FrameCounter;
                if (_FrameCounter >= frame.DisplayTime)
                {
                    ++_CurrentFrameIndex;
                    _FrameCounter = 0;
                }

                pictureBox1.Invalidate();
            }
            else if (_Image != null)
            {
                _Image = null;
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            if (_Image != null)
            {
                e.Graphics.DrawImage(_Image, _Position);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                _SelectedAnimation = null;
            }
            else
            {
                SetSelectedAnimation(listView1.SelectedItems[0].Tag as GSPat.Animation);
            }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0)
            {
                _SelectedAnimation = null;
            }
            else
            {
                SetSelectedAnimation(listView2.SelectedItems[0].Tag as GSPat.Animation);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (_Project != null && _GSPatFile != null)
            {
                AddPathToProject(_Path, FormCodeRes.ImportPatAnimationForm_ProjPath);
                if (_Project.Settings.Palettes == null || _Project.Settings.Palettes.Count == 0)
                {
                    _Project.Settings.Palettes = new List<string>()
                    {
                        Path.GetFileName(_Palette),
                    };
                    AddPathToProject(Path.GetDirectoryName(_Palette), FormCodeRes.ImportPatAnimationForm_ProjPathPal);
                }
                ImportedSegments = listView2.Items.OfType<ListViewItem>()
                    .Select(i => ProjectGenerater.ImportSegment(_Project, _GSPatFile, (GSPat.Animation)i.Tag))
                    .ToList();
            }
        }

        //TODO move to project
        private void AddPathToProject(string path, string prefix)
        {
            if (!_Project.Settings.Directories.Any(d =>
                d.Usage == Pat.ProjectDirectoryUsage.Image &&
                d.Path == path))
            {
                int id = 1;
                while (_Project.Settings.Directories.Any(d => d.Name == prefix + id))
                {
                    ++id;
                }
                //TODO make it easier to insert something with a name into a list
                _Project.Settings.Directories.Add(new Pat.ProjectDirectoryDesc()
                {
                    Name = prefix + id,
                    Usage = Pat.ProjectDirectoryUsage.Image,
                    Path = path,
                });
            }
        }
    }
}
