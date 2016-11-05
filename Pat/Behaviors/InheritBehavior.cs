using GS_PatEditor.Editor.Editable;
using GS_PatEditor.Pat.Effects;
using GS_PatEditor.Pat.Effects.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat.Behaviors
{
    [Serializable]
    [LocalizedClassDisplayName(typeof(InheritBehavior))]
    public class InheritBehavior : Behavior, IEditableEnvironment
    {
        [XmlElement]
        [TypeConverter(typeof(ActionIDConverter))]
        [LocalizedDescriptionAttribute("InheritBehavior_Action")]
        public string Action { get; set; }

        public override void MakeEffects(ActionEffects effects)
        {
            var behaviors = effects.GetActionByName(Action).Behaviors;
            foreach (var b in behaviors)
            {
                b.MakeEffects(effects);
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        public EditableEnvironment Environment { get; set; }
    }
}
