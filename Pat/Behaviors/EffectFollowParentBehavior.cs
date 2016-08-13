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
    public class EffectFollowParentBehavior : Behavior
    {
        [XmlElement]
        public int Segment { get; set; }

        [XmlElement]
        public string SingleInstanceID { get; set; }

        [XmlElement]
        public bool FailIfParentMotionChanged { get; set; }

        [XmlElement]
        public bool ReleaseIfCheckFailed { get; set; }

        [XmlElement]
        public int? SegmentCheckFailed { get; set; }

        public override void MakeEffects(ActionEffects effects)
        {
            effects.SegmentStartEffects.AddEffectToList(Segment, new Effects.BulletFollowingOwnerInitEffect
            {
                CheckInstance = SingleInstanceID,
            });
            effects.UpdateEffects.Add(new FilteredEffect
            {
                Filter = new Effects.AnimationSegmentFilter { Segment = Segment },
                Effect = new Effects.BulletFollowingOwnerUpdateEffect
                {
                    CheckInstance = SingleInstanceID,
                    FailIfParentMotionChanged = FailIfParentMotionChanged,
                    ReleaseIfCheckFailed = ReleaseIfCheckFailed,
                    SegmentCheckFailed = SegmentCheckFailed,
                },
            });
        }
    }

    [Serializable]
    public class ClearFollowingEffectBehavior : Behavior
    {
        [XmlElement]
        [EditorChildNode("Time")]
        public Time Time;

        [XmlElement]
        public string SingleInstanceID { get; set; }

        public override void MakeEffects(ActionEffects effects)
        {
            Time.MakeEffects(effects, new Effects.ActorSetFloatVariableEffect
            {
                Name = SingleInstanceID,
                Value = new ConstValue { Value = 0 },
            });
        }
    }
}
