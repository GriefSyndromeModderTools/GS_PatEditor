﻿using System;
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
    public partial class AnimationPropertyFrom : Form
    {
        public AnimationPropertyFrom()
        {
            InitializeComponent();
        }

        public string AnimationID
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
            }
        }
    }
}
