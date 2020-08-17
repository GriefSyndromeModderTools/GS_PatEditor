using GS_PatEditor.Pat;
using GS_PatEditor.Pat.Effects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Editor.Exporters.Enemy
{
    [Serializable]
    public class EnemyExporter : AbstractExporter, IEditableEnvironment
    {
        [XmlElement]
        public int BaseIndex { get; set; }

        [XmlElement]
        public string EnemyName { get; set; }

        [XmlElement]
        public string ScriptFileName { get; set; }

        [XmlElement]
        public bool NoPatTail { get; set; }

        [XmlElement]
        public int RandomRange { get; set; }

        [XmlElement]
        public bool ReorderAnimation { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public EditableEnvironment Environment { get; set; }

        public override void Export(Project proj)
        {
            System.Windows.Forms.MessageBox.Show("Export test");
        }

        public override void ShowOptionDialog(Project proj)
        {
            Environment = new EditableEnvironment(proj);

            var dialog = new EnemyExporterOptionsForm(proj, this);
            dialog.ShowDialog();
        }
    }
}
