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
    public partial class CloneHitBoxDialog : Form
    {
        public CloneHitBoxDialog(string[] animations)
        {
            InitializeComponent();
            foreach (var a in animations)
            {
                comboBox1.Items.Add(a);
            }
        }

        public bool MatchLocation { get; private set; }
        public string SelectedAnimationName { get; private set; }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            MatchLocation = checkBox1.Checked;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedAnimationName = comboBox1.Text;
            button1.Enabled = SelectedAnimationName != null;
        }
    }
}
