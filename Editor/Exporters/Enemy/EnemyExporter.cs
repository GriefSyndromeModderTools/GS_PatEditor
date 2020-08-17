using GS_PatEditor.Pat;
using GS_PatEditor.Pat.Effects;
using GS_PatEditor.Pat.Effects.Converter;
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
    public class EnemyExporterAnimations : IEditableEnvironment
    {
        [XmlElement]
        [TypeConverter(typeof(ActionIDConverterNullable))]
        public string DamageLight { get; set; }

        [XmlElement]
        [TypeConverter(typeof(ActionIDConverterNullable))]
        public string DamageHeavy { get; set; }
        
        [XmlElement]
        [TypeConverter(typeof(ActionIDConverterNullable))]
        public string DamageSmash { get; set; }

        [XmlElement]
        [TypeConverter(typeof(ActionIDConverterNullable))]
        public string Dead { get; set; }

        [XmlElement]
        [TypeConverter(typeof(ActionIDConverterNullable))]
        public string DeadSmash { get; set; }

        //TODO initial action list

        [XmlIgnore]
        [Browsable(false)]
        public EditableEnvironment Environment { get; set; }
    }

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

        [XmlElement]
        public EnemyExporterAnimations Animations = new EnemyExporterAnimations();

        [XmlArray]
        public List<ActionList> ActionLists = new List<ActionList>();

        public override void Export(Project proj)
        {
            System.Windows.Forms.MessageBox.Show("Export test");
        }

        public override void ShowOptionDialog(Project proj)
        {
            Environment = new EditableEnvironment(proj);
            Animations.Environment = Environment;

            var dialog = new EnemyExporterOptionsForm(proj, this);
            dialog.ShowDialog();
        }

        [XmlIgnore]
        [Browsable(false)]
        public EditableEnvironment Environment { get; set; }
    }
}
