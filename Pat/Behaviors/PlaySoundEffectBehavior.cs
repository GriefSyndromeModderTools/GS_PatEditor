using GS_PatEditor.Editor.Editable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat.Behaviors
{
    [Serializable]
    [LocalizedClassDisplayName(typeof(PlaySoundEffectBehavior))]
    public class PlaySoundEffectBehavior : Behavior
    {
        [XmlElement]
        [LocalizedDescriptionAttribute("PlaySoundEffectBehavior_Index")]
        public int Index { get; set; }

        [XmlElement]
        [EditorChildNode("PlaySoundEffectBehavior_Time")]
        public Time Time;

        public override void MakeEffects(ActionEffects effects)
        {
            Time.MakeEffects(effects, new Effects.SoundEffect { Index = Index });
        }
    }
}
