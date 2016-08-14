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
    [LocalizedClassDisplayName(typeof(PlayerSkillLoopBreakBehavior))]
    public class PlayerSkillLoopBreakBehavior : Behavior
    {
        [XmlAttribute]
        [LocalizedDescriptionAttribute("PlayerSkillLoopBreakBehavior_Segment")]
        public int Segment { get; set; }

        [XmlAttribute]
        [LocalizedDescriptionAttribute("PlayerSkillLoopBreakBehavior_Tick")]
        public int Tick { get; set; }

        [XmlAttribute]
        [LocalizedDescriptionAttribute("PlayerSkillLoopBreakBehavior_KeyReleased")]
        public bool KeyReleased { get; set; }

        [XmlElement]
        [LocalizedDescriptionAttribute("PlayerSkillLoopBreakBehavior_SegmentTo")]
        public int? SegmentTo { get; set; }

        public override void MakeEffects(ActionEffects effects)
        {
            var breakEffect = new SimpleListEffect();
            if (SegmentTo.HasValue)
            {
                breakEffect.EffectList.Add(new Effects.SetSegmentEffect { Segment = SegmentTo.Value });
            }
            else
            {
                breakEffect.EffectList.Add(new Effects.AnimationContinueEffect());
            }
            var filter = new SimpleListFilter();
            filter.FilterList.Add(new Effects.AnimationSegmentFilter { Segment = Segment });
            filter.FilterList.Add(new Effects.AnimationCountAfterFilter { Count = new ConstValue { Value = Tick } });
            if (KeyReleased)
            {
                filter.FilterList.Add(new Effects.PlayerKeyReleasedFilter());
            }
            effects.UpdateEffects.Add(new FilteredEffect { Filter = filter, Effect = breakEffect });
        }
    }
}
