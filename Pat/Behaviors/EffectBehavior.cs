using GS_PatEditor.Editor.Editable;
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
    [LocalizedClassDisplayName(typeof(EffectBehavior))]
    public class EffectBehavior : Behavior
    {
        [XmlElement]
        [EditorChildNode("EffectBehavior_Time")]
        public Time Time;

        [XmlElement]
        [EditorChildNode("EffectBehavior_Effect")]
        public Effect Effect;

        public override void MakeEffects(ActionEffects effects)
        {
            Time.MakeEffects(effects, Effect);
        }
    }
}
