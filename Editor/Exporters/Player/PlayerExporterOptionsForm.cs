using GS_PatEditor.Pat.Effects;
using System;
using System.Collections.Generic;
using GS_PatEditor.Editor.Editable;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GS_PatEditor.Localization;

namespace GS_PatEditor.Editor.Exporters.Player
{
    partial class PlayerExporterOptionsForm : Form
    {
        public PlayerExporterOptionsForm(Pat.Project proj, PlayerExporter exporter)
        {
            InitializeComponent();

            treeView1.LinkedPropertyGrid = propertyGrid1;
            treeView1.LinkedDeleteButton = button1;
            treeView1.LinkedResetButton = button2;
            treeView1.LinkedMoveDownButton = button3;
            treeView1.LinkedMoveUpButton = button4;

            var env = new EditableEnvironment(proj);

            treeView1.Nodes.Add(new TreeNode
            {
                Text = PlayerExporterRes.Item_ExportOptions,
                Tag = exporter,
            });

            treeView1.Nodes.Add(new TreeNode
            {
                Text = PlayerExporterRes.Item_SystemAnimations,
                Tag = exporter.Animations,
            });

            treeView1.Nodes.Add(new TreeNode
            {
                Text = PlayerExporterRes.Item_PlayerInformation,
                Tag = exporter.PlayerInformation,
            });

            var skills = new TreeNode
            {
                Text = PlayerExporterRes.Item_Skills,
            };
            treeView1.Nodes.Add(skills);
            skills.Nodes.AddEditableList(env, exporter.Skills);
        }
    }
}
