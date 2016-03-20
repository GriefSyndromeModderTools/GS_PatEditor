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
    public partial class EffectListEditForm : Form
    {
        private Pat.EffectList _Effects;

        public EffectListEditForm(Pat.EffectList effects)
        {
            InitializeComponent();
            treeView1.Tag = (Action)UpdateSelectedNode;

            _Effects = effects;
            RefreshList();
        }

        private void RefreshList()
        {
            treeView1.Nodes.Clear();
            foreach (var effect in _Effects)
            {
                treeView1.Nodes.Add(EffectTreeNodeHandler.CreateNode(effect,
                    new ListMultiEditable<Pat.Effect> { List = _Effects.Effects }));
            }
            treeView1.Nodes.Add(EffectTreeNodeHandler.CreateFinalEffectNode(
                new ListMultiEditable<Pat.Effect> { List = _Effects.Effects }));
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView1.SelectedNode = e.Node;
            UpdateSelectedNode();
        }

        private void UpdateSelectedNode()
        {
            var node = treeView1.SelectedNode;
            if (node == null)
            {
                propertyGrid1.SelectedObject = null;
                button1.Enabled = false;
                button2.Enabled = false;
                return;
            }

            propertyGrid1.SelectedObject = node.Tag;

            var nnode = node as EditableTreeNode;
            if (nnode != null)
            {
                button2.Enabled = nnode.CanReset;
                button1.Enabled = nnode.CanDelete;
            }
            else
            {
                button2.Enabled = false;
                button1.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode as EditableTreeNode;
            if (node != null)
            {
                node.Reset();
            }
        }
    }
}
