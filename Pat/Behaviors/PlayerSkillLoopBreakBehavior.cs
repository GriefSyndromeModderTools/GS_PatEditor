using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat.Behaviors
{
    [Serializable]
    public class PlayerSkillLoopBreakBehavior : Behavior
    {
        [XmlAttribute]
        public int Segment { get; set; }

        [XmlAttribute]
        public int Tick { get; set; }

        [XmlAttribute]
        public bool KeyReleased { get; set; }

        [XmlElement]
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
