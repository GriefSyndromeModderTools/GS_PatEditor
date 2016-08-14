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
    public partial class ProjectDirectoryEditForm : Form
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var setting = new Pat.ProjectSettings()
            {
                Directories = new List<Pat.ProjectDirectoryDesc>()
                {
                    new Pat.ProjectDirectoryDesc()
                    {
                        Name = "images",
                        Usage = Pat.ProjectDirectoryUsage.Image,
                    },
                },
            };
            var f = new ProjectDirectoryEditForm(setting, true);
            Application.Run(f);
        }

        private readonly Pat.ProjectSettings _ProjectSetting;
        private readonly bool _AllowEditEntry;

        private Pat.ProjectDirectoryDesc _SelectedDir;

        public ProjectDirectoryEditForm(Pat.ProjectSettings setting, bool allowEditEntry)
        {
            InitializeComponent();

            _ProjectSetting = setting;
            _AllowEditEntry = allowEditEntry;
            if (!allowEditEntry)
            {
                buttonAdd.Enabled = false;
                buttonRemove.Enabled = false;
            }

            RefreshList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_SelectedDir != null)
            {
                if (Directory.Exists(_SelectedDir.Path))
                {
                    folderBrowserDialog1.SelectedPath = _SelectedDir.Path;
                }
                else
                {
                    folderBrowserDialog1.SelectedPath = String.Empty;
                }
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    _SelectedDir.Path = folderBrowserDialog1.SelectedPath;
                    textBox2.Text = _SelectedDir.Path;
                    UpdateList(listView1);
                    UpdateList(listView2);
                }
            }
        }

        private void UpdateSelected(ListView list)
        {
            if (list.SelectedItems.Count == 0)
            {
                _SelectedDir = null;
            }
            else
            {
                _SelectedDir = list.SelectedItems.OfType<ListViewItem>().First().Tag
                    as Pat.ProjectDirectoryDesc;
            }

            if (_SelectedDir == null)
            {
                textBox1.Text = String.Empty;
                textBox2.Text = String.Empty;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                button1.Enabled = false;
            }
            else
            {
                textBox1.Text = _SelectedDir.Name;
                textBox2.Text = _SelectedDir.Path;
                textBox1.Enabled = _AllowEditEntry;
                textBox2.Enabled = true;
                button1.Enabled = true;
            }
        }

        private void UpdateList(ListView list)
        {
            if (list.SelectedItems.Count > 0)
            {
                var item = list.SelectedItems.OfType<ListViewItem>().First();
                if (item.Tag == _SelectedDir)
                {
                    item.Text = _SelectedDir.Name;
                    item.SubItems[1].Text = _SelectedDir.Path;
                }
            }
        }

        private void RefreshList()
        {
            listView1.Items.Clear();
            listView2.Items.Clear();

            ListViewItem selected = null;
            foreach (var dir in _ProjectSetting.Directories)
            {
                ListViewItem item = new ListViewItem(new string[] { dir.Name, dir.Path }, -1);
                item.ToolTipText = dir.Path;
                item.Tag = dir;
                if (dir.Usage == Pat.ProjectDirectoryUsage.Image)
                {
                    listView1.Items.Add(item);
                }
                else
                {
                    listView2.Items.Add(item);
                }
                if (_SelectedDir == dir)
                {
                    selected = item;
                    item.Selected = true;
                }
            }

            if (selected != null)
            {
                selected.EnsureVisible();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelected(listView1);
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelected(listView2);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (_AllowEditEntry && _SelectedDir != null)
            {
                _SelectedDir.Name = textBox1.Text;
                UpdateList(listView1);
                UpdateList(listView2);
            }
        }
        
        private string FindFreeName()
        {
            var prefix = FormCodeRes.ProjectDirectoryEditForm_FreeNamePrefix;
            int index = 1;
            while (_ProjectSetting.Directories.Any(d => d.Name == prefix + index))
            {
                ++index;
            }
            return prefix + index;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var name = FindFreeName();
            if (tabControl1.SelectedIndex == 0)
            {
                _ProjectSetting.Directories.Add(new Pat.ProjectDirectoryDesc()
                {
                    Name = name,
                    Usage = Pat.ProjectDirectoryUsage.Image,
                    Path = String.Empty,
                });
            }
            else
            {
                _ProjectSetting.Directories.Add(new Pat.ProjectDirectoryDesc()
                {
                    Name = name,
                    Usage = Pat.ProjectDirectoryUsage.SoundEffect,
                    Path = String.Empty,
                });
            }

            RefreshList();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            ListView list = null;
            if (tabControl1.SelectedIndex == 0)
            {
                list = listView1;
            }
            else
            {
                list = listView2;
            }
            if (list.SelectedItems.Count > 0)
            {
                int index = list.SelectedIndices[0];
                _ProjectSetting.Directories.RemoveAt(index);
                list.Items.RemoveAt(index);
                UpdateSelected(list);
            }
        }
    }
}
