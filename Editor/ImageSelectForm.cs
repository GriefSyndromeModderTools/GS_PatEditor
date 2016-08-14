using GS_PatEditor.Localization;
using GS_PatEditor.Pat;
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
    public partial class ImageSelectForm : Form
    {
        private Pat.Project _Project;
        private Bitmap _PreviewImage;
        private bool _IsBatch;

        public ImageSelectForm(Pat.Project project)
        {
            InitializeComponent();

            _Project = project;
            RefreshList();
        }

        private void RefreshList()
        {
            listView1.Items.Clear();
            if (!_IsBatch)
            {
                foreach (var image in _Project.Images)
                {
                    listView1.Items.Add(CreateItem(image));
                }
            }
            if (checkBox1.Checked || _IsBatch)
            {
                var dirList = _Project.Settings.Directories
                    .Where(d => d.Usage == ProjectDirectoryUsage.Image && d.Path != null && d.Path.Length > 0)
                    .Select(d => d.Path);
                foreach (var dir in dirList)
                {
                    if (!Directory.Exists(dir))
                    {
                        continue;
                    }
                    //TODO handle duplicate filename
                    foreach (var image in Directory.EnumerateFiles(dir, "*.cv2", SearchOption.TopDirectoryOnly))
                    {
                        listView1.Items.Add(CreateFile(image));
                    }
                    foreach (var image in Directory.EnumerateFiles(dir, "*.dds", SearchOption.TopDirectoryOnly))
                    {
                        listView1.Items.Add(CreateFile(image));
                    }
                }
            }
        }

        private ListViewItem CreateFile(string file)
        {
            var filename = Path.GetFileName(file);
            var ret = new ListViewItem(new string[] { FormCodeRes.ImageSelectForm_CreateItem, filename }, -1);
            ret.StateImageIndex = 0;
            ret.ForeColor = Color.Gray;
            ret.Tag = file;
            ret.ToolTipText = file;
            return ret;
        }

        private ListViewItem CreateItem(Pat.FrameImage img)
        {
            var ret = new ListViewItem(new string[] { img.ImageID, img.Resource.ResourceID }, -1);
            ret.StateImageIndex = 0;
            ret.Tag = img;
            return ret;
        }

        private ListViewItem GetSelectedItem()
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return null;
            }
            return listView1.SelectedItems[0];
        }

        public string SelectedImage
        {
            get
            {
                var sel = GetSelectedItem();
                if (sel == null)
                {
                    return null;
                }
                return sel.Text;
            }
            set
            {
                var item = listView1.Items.Cast<ListViewItem>()
                    .FirstOrDefault(i => i.Text == value);
                listView1.SelectedItems.Clear();
                if (item != null)
                {
                    item.Selected = true;
                    item.EnsureVisible();
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_IsBatch)
            {
                return;
            }

            if (listView1.SelectedItems.Count == 0)
            {
                textBox1.Text = String.Empty;
                textBox1.Enabled = false;
                buttonOK.Enabled = false;
            }
            else if (listView1.SelectedItems[0].Tag is Pat.FrameImage)
            {
                _PreviewImage = null;
                textBox1.Enabled = true;
                buttonOK.Enabled = true;
                textBox1.ReadOnly = false;
                //image item
                var img = (Pat.FrameImage)listView1.SelectedItems[0].Tag;
                SetupImage(img);
                checkBox2.Checked = img.AlphaBlendMode;
            }
            else if (listView1.SelectedItems[0].Tag is string)
            {
                textBox1.Enabled = true;
                buttonOK.Enabled = true;
                textBox1.ReadOnly = true;
                buttonOK.Enabled = false;
                var file = (string)listView1.SelectedItems[0].Tag;
                //TODO handle duplicate file
                textBox1.Text = Path.GetFileNameWithoutExtension(file);

                //show preview
                _PreviewImage = _Project.ImageList.GetImageUnclippedByRes(Path.GetFileName(file), false);
                checkBox2.Checked = false;

                pictureBox1.Invalidate();
            }
        }

        private void SetupImage(Pat.FrameImage img)
        {
            textBox1.Text = img.ImageID;
            pictureBox1.Invalidate();
        }

        //TODO implement this in ImageList
        private Pat.FrameImage MakeNewImageFromFile(string str)
        {
            if (str == null || str.Length == 0 || !File.Exists(str))
            {
                return null;
            }

            var filenameWithoutPath = Path.GetFileName(str);
            Images.AbstractImage img;
            if (Path.GetExtension(str) == ".cv2")
            {
                img = new Images.CV2Image(str);
            }
            else if (Path.GetExtension(str) == ".dds")
            {
                img = new Images.DDSImage(str);
            }
            else
            {
                //error
                return null;
            }

            if (img == null)
            {
                return null;
            }

            var frame = new GSPat.Frame()
            {
                ViewOffsetX = 0,
                ViewOffsetY = 0,
                ViewWidth = (short)img.Width,
                ViewHeight = (short)img.Height,
            };
            img.Dispose();
            var image = ProjectGenerater.AddImageToProject(_Project, filenameWithoutPath, frame);

            if (!_IsBatch)
            {
                var item = CreateItem(image);
                listView1.Items.Add(item);
                listView1.SelectedItems.Clear();
                item.Selected = true;
                item.EnsureVisible();
            }
            return image;
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            if (listView1.SelectedItems[0].Tag is string)
            {
                var str = (string)listView1.SelectedItems[0].Tag;
                if (MessageBox.Show(FormCodeRes.ImageSelectForm_ConfirmCreate, FormCodeRes.ImageSelectForm_MsgBoxTitle,
                    MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    var img = MakeNewImageFromFile(str);
                    if (img == null)
                    {
                        return;
                    }
                    textBox1.Text = img.ImageID;
                    textBox1.ReadOnly = false;
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 0 && listView1.SelectedItems[0].Tag is Pat.FrameImage)
            {
                var image = (Pat.FrameImage)listView1.SelectedItems[0].Tag;
                var oldValue = image.ImageID;
                var newValue = textBox1.Text;
                if (oldValue != newValue)
                {
                    //replace all image ids
                    //TODO move to ImageList
                    image.ImageID = newValue;
                    foreach (var action in _Project.Actions)
                    {
                        if (action.ImageID == oldValue)
                        {
                            action.ImageID = newValue;
                        }
                    }
                    foreach (var frame in _Project.Actions
                        .SelectMany(p => p.Segments)
                        .SelectMany(s => s.Frames))
                    {
                        if (frame.ImageID == oldValue)
                        {
                            frame.ImageID = newValue;
                        }
                    }

                    listView1.SelectedItems[0].Text = newValue;
                }
            }
        }

        private void ImageSelectForm_Load(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 0)
            {
                listView1.SelectedItems[0].EnsureVisible();
            }
        }

        private float _ViewOffsetX = 0, _ViewOffsetY = 0, _ViewScale = 1;

        private Point PointSpriteToClient(Point p)
        {
            var size = pictureBox1.ClientRectangle.Size;
            return new Point(
                size.Width / 2 + (int)((_ViewOffsetX + p.X) * _ViewScale),
                size.Height / 2 + (int)((_ViewOffsetY + p.Y) * _ViewScale));
        }

        private Point PointClientToSprite(Point p)
        {
            var size = pictureBox1.ClientRectangle.Size;
            var pp = new Point(p.X - size.Width / 2, p.Y - size.Height / 2);
            return new Point((int)(pp.X / _ViewScale - _ViewOffsetX), (int)(pp.Y / _ViewScale - _ViewOffsetY));
        }

        private bool _IsMouseDownDraw, _IsMouseDownMove;
        private float _ViewMoveStartX, _ViewMoveStartY;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _IsMouseDownDraw = true;

                if (listView1.SelectedItems.Count != 0 && listView1.SelectedItems[0].Tag is Pat.FrameImage)
                {
                    var image = (Pat.FrameImage)listView1.SelectedItems[0].Tag;
                    var img = _Project.ImageList.GetImageUnclipped(image.ImageID);
                    if (img == null)
                    {
                        return;
                    }

                    var p0 = PointClientToSprite(e.Location);
                    p0.X += img.Width / 2;
                    p0.Y += img.Height / 2;
                    if (p0.X < 0)
                    {
                        p0.X = 0;
                    }
                    if (p0.Y < 0)
                    {
                        p0.Y = 0;
                    }
                    image.X = p0.X;
                    image.Y = p0.Y;
                    image.W = 1;
                    image.H = 1;

                    pictureBox1.Invalidate();
                }
            }
            if (e.Button == MouseButtons.Middle)
            {
                _IsMouseDownMove = true;
                _ViewMoveStartX = _ViewOffsetX - e.X;
                _ViewMoveStartY = _ViewOffsetY - e.Y;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButtons.Left) && _IsMouseDownDraw)
            {
                _IsMouseDownDraw = false;

                if (listView1.SelectedItems.Count != 0 && listView1.SelectedItems[0].Tag is Pat.FrameImage)
                {
                    var image = (Pat.FrameImage)listView1.SelectedItems[0].Tag;
                    _Project.ImageList.ResetImage(image);
                    pictureBox1.Invalidate();
                }
            }
            if (e.Button.HasFlag(MouseButtons.Middle) && _IsMouseDownMove)
            {
                _IsMouseDownMove = false;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_IsMouseDownMove)
            {
                _ViewOffsetX = _ViewMoveStartX + e.X;
                _ViewOffsetY = _ViewMoveStartY + e.Y;
                pictureBox1.Invalidate();
            }
            else if (_IsMouseDownDraw)
            {
                if (listView1.SelectedItems.Count != 0 && listView1.SelectedItems[0].Tag is Pat.FrameImage)
                {
                    var image = (Pat.FrameImage)listView1.SelectedItems[0].Tag;
                    var img = _Project.ImageList.GetImageUnclipped(image.ImageID);
                    if (img == null)
                    {
                        return;
                    }

                    var p0 = PointClientToSprite(e.Location);
                    p0.X += img.Width / 2;
                    p0.Y += img.Height / 2;
                    if (p0.X > img.Width)
                    {
                        p0.X = img.Width;
                    }
                    if (p0.Y > img.Height)
                    {
                        p0.Y = img.Height;
                    }
                    p0.X -= image.X;
                    p0.Y -= image.Y;
                    image.W = p0.X;
                    image.H = p0.Y;

                    pictureBox1.Invalidate();
                }
            }
        }

        private static Pen _PenDrawRect = new Pen(Color.Black, 2);
        private static Brush _BrushImage = Brushes.White;

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.LightGray);
            if (listView1.SelectedItems.Count != 0 && listView1.SelectedItems[0].Tag is Pat.FrameImage)
            {
                var image = (Pat.FrameImage)listView1.SelectedItems[0].Tag;
                var img = _Project.ImageList.GetImageUnclipped(image.ImageID);
                if (img == null)
                {
                    return;
                }

                var p0 = PointSpriteToClient(new Point(-img.Width / 2, -img.Height / 2));
                var p1 = PointSpriteToClient(new Point(img.Width / 2, img.Height / 2));

                e.Graphics.FillRectangle(_BrushImage, p0.X, p0.Y, p1.X - p0.X, p1.Y - p0.Y);
                e.Graphics.DrawImage(img, p0);

                var pp0 = PointSpriteToClient(new Point(-img.Width / 2 + image.X, -img.Height / 2 + image.Y));
                var pp1 = PointSpriteToClient(new Point(-img.Width / 2 + image.X + image.W, -img.Height / 2 + image.Y + image.H));

                e.Graphics.DrawRectangle(_PenDrawRect, pp0.X, pp0.Y, pp1.X - pp0.X, pp1.Y - pp0.Y);
            }
            else if (_PreviewImage != null)
            {
                var p0 = PointSpriteToClient(new Point(-_PreviewImage.Width / 2, -_PreviewImage.Height / 2));
                var p1 = PointSpriteToClient(new Point(_PreviewImage.Width / 2, _PreviewImage.Height / 2));

                e.Graphics.DrawImage(_PreviewImage, p0);
            }
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 0 && listView1.SelectedItems[0].Tag is Pat.FrameImage)
            {
                var img = (Pat.FrameImage)listView1.SelectedItems[0].Tag;
                img.AlphaBlendMode = checkBox2.Checked;
                _Project.ImageList.ResetImage(img);
                pictureBox1.Invalidate();
            }
            else if (listView1.SelectedItems.Count != 0 && listView1.SelectedItems[0].Tag is string &&
                _PreviewImage != null)
            {
                var img = (string)listView1.SelectedItems[0].Tag;
                _PreviewImage = _Project.ImageList.GetImageUnclippedByRes(Path.GetFileName(img), checkBox2.Checked);
                pictureBox1.Invalidate();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!_IsBatch)
            {
                _IsBatch = true;
                //begin
                button2.Text = FormCodeRes.ImageSelectForm_Import;

                textBox1.Enabled = false;
                checkBox1.Enabled = false;
                checkBox2.Enabled = false;
                buttonOK.Enabled = false;
                buttonCancel.Enabled = false;
                button1.Enabled = false;

                RefreshList();
                listView1.CheckBoxes = true;
            }
            else
            {
                var list = listView1.Items
                    .OfType<ListViewItem>()
                    .Where(i => i.Checked)
                    .Select(i => (string)i.Tag).ToArray();
                foreach (var f in list)
                {
                    MakeNewImageFromFile(f);
                }

                _IsBatch = false;

                //finish
                button2.Text = FormCodeRes.ImageSelectForm_BatchImport;

                textBox1.Enabled = true;
                checkBox1.Enabled = true;
                checkBox2.Enabled = true;
                buttonOK.Enabled = true;
                buttonCancel.Enabled = true;
                button1.Enabled = true;

                RefreshList();
                listView1.CheckBoxes = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_IsBatch)
            {
                return;
            }

            if (listView1.SelectedItems.Count != 0 && listView1.SelectedItems[0].Tag is Pat.FrameImage)
            {
                var img = (Pat.FrameImage)listView1.SelectedItems[0].Tag;
                if (CheckImageUsage(img.ImageID))
                {
                    MessageBox.Show(FormCodeRes.ImageSelectForm_RemoveInUse,
                        FormCodeRes.ImageSelectForm_MsgBoxTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    if (MessageBox.Show(FormCodeRes.ImageSelectForm_ConfirmRemove,
                        FormCodeRes.ImageSelectForm_MsgBoxTitle,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                    {
                        _Project.Images.Remove(img);
                        _Project.ImageList.ResetImage(img);

                        listView1.SelectedItems[0].Remove();
                        listView1.SelectedItems.Clear();
                    }
                }
            }
            else
            {
                MessageBox.Show(FormCodeRes.ImageSelectForm_NotCreated,
                    FormCodeRes.ImageSelectForm_MsgBoxTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private bool CheckImageUsage(string img)
        {
            if (_Project.Exporter != null)
            {
                if (_Project.Exporter.IsImageIDDirectlyUsed(img))
                {
                    return true;
                }
            }
            foreach (var action in _Project.Actions)
            {
                foreach (var seg in action.Segments)
                {
                    foreach (var frame in seg.Frames)
                    {
                        if (frame.ImageID == img)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
