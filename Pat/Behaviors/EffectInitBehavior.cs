using GS_PatEditor.Editor.Editable;
using GS_PatEditor.Pat.Effects;
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
    [LocalizedClassDisplayName(typeof(EffectInitBehavior))]
    public class EffectInitBehavior : Behavior
    {
        [XmlAttribute]
        [DefaultValue(false)]
        public bool ImmuneToTimeStop { get; set; }

        public override void MakeEffects(ActionEffects effects)
        {
            effects.InitEffects.Add(new BulletInitEffect());
            effects.InitEffects.Add(new InitCountEffect());
            if (!ImmuneToTimeStop) effects.UpdateEffects.Add(new TimeStopCheckEffect());
            effects.UpdateEffects.Add(new IncreaseCountEffect());
            effects.SegmentFinishEffects.AddEffectToList(effects.SegmentCount - 1,
                new Effects.ReleaseActorEffect());
        }
    }
}
