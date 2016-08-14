using GS_PatEditor.Localization;
using GS_PatEditor.Pat.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor.Editable
{
    static class TreeNodeExt
    {
        public static TreeNodeCollection GetParentCollection(this TreeNode node)
        {
            if (node.Parent == null)
            {
                return node.TreeView.Nodes;
            }
            return node.Parent.Nodes;
        }

        public static void Replace(this TreeNode node, TreeNode newNode)
        {
            var shouldSetSelected = node.IsSelected;

            var coll = node.GetParentCollection();
            var index = coll.IndexOf(node);
            if (index == -1)
            {
                return;
            }
            coll.RemoveAt(index);
            coll.Insert(index, newNode);

            if (shouldSetSelected)
            {
                ((EditableTreeView)newNode.TreeView).SetSelectedNodeWithCallback(newNode);
            }
        }

        public static void RemoveFromParent(this TreeNode node)
        {
            node.GetParentCollection().Remove(node);
        }

        public static void InsertBefore(this TreeNode node, TreeNode newNode)
        {
            var coll = node.GetParentCollection();
            var index = coll.IndexOf(node);
            if (index == -1)
            {
                return;
            }
            coll.Insert(index, newNode);
        }

        public static TreeNode CreateNodeWithChild(string text, TreeNode child)
        {
            var node = new TreeNode { Text = text };
            node.Nodes.Add(child);
            return node;
        }

        public static TreeNode CreateNode(Pat.Filter filter)
        {
            return new TreeNode()
            {
                Text = filter == null ? EditableTreeViewRes.Null : filter.GetType().Name,
                Tag = filter
            };
        }

        public static void AddEditableList<T>(this TreeNodeCollection coll,
            EditableEnvironment env, List<T> list)
            where T : class
        {
            var me = new ListMultiEditable<T>(list);
            foreach (var item in list)
            {
                coll.Add(EditableNodeGenerator.Create<T>(env, item, me));
            }
            coll.Add(EditableNodeGenerator.Create<T>(env, me));
        }

        public static void NodeMoveUp(this TreeNode node)
        {
            var coll = node.GetParentCollection();
            var index = coll.IndexOf(node);
            if (index == -1 || index == 0)
            {
                return;
            }
            coll.RemoveAt(index);
            coll.Insert(index - 1, node);
        }

        public static void NodeMoveDown(this TreeNode node)
        {
            var coll = node.GetParentCollection();
            var index = coll.IndexOf(node);
            if (index == -1 || index == coll.Count - 1)
            {
                return;
            }
            coll.RemoveAt(index);
            coll.Insert(index + 1, node);
        }
    }
}
