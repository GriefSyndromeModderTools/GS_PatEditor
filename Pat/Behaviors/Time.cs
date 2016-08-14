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
    [SerializationBaseClassAttribute]
    public abstract class Time
    {
        public abstract void MakeEffects(ActionEffects dest, Effect effect);
    }

    [Serializable]
    [LocalizedClassDisplayName(typeof(TimeStart))]
    public class TimeStart : Time
    {
        public override void MakeEffects(ActionEffects dest, Effect effect)
        {
            dest.InitEffects.Add(effect);
        }
    }

    [Serializable]
    [LocalizedClassDisplayName(typeof(TimeStartSegment))]
    public class TimeStartSegment : Time
    {
        [XmlAttribute]
        [LocalizedDescriptionAttribute("TimeStartSegment_Segment")]
        public int Segment { get; set; }

        public override void MakeEffects(ActionEffects dest, Effect effect)
        {
            dest.SegmentStartEffects.AddEffectToList(Segment, effect);
        }
    }

    [Serializable]
    [LocalizedClassDisplayName(typeof(TimeEndSegment))]
    public class TimeEndSegment : Time
    {
        [XmlAttribute]
        [LocalizedDescriptionAttribute("TimeEndSegment_Segment")]
        public int Segment { get; set; }

        public override void MakeEffects(ActionEffects dest, Effect effect)
        {
            dest.SegmentFinishEffects.AddEffectToList(Segment, effect);
        }
    }

    [Serializable]
    [LocalizedClassDisplayName(typeof(TimeRepeat))]
    public class TimeRepeat : Time
    {
        [XmlAttribute]
        [LocalizedDescriptionAttribute("TimeRepeat_Segment")]
        public int Segment { get; set; }
        [XmlAttribute]
        [LocalizedDescriptionAttribute("TimeRepeat_Interval")]
        public int Interval { get; set; }

        public override void MakeEffects(ActionEffects dest, Effect effect)
        {
            var interval = new ConstValue { Value = Interval };
            dest.UpdateEffects.Add(new FilteredEffect()
            {
                Filter = new SimpleListFilter(
                    new Effects.AnimationSegmentFilter { Segment = Segment },
                    new Effects.AnimationCountModFilter { Divisor = interval, Mod = new ConstValue { Value = 0 } }
                ),
                Effect = effect,
            });
        }
    }
}
