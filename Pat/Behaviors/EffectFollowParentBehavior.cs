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
    [LocalizedClassDisplayName(typeof(EffectFollowParentBehavior))]
    public class EffectFollowParentBehavior : Behavior
    {
        [XmlElement]
        [LocalizedDescriptionAttribute("EffectFollowParentBehavior_Segment")]
        public int Segment { get; set; }

        [XmlElement]
        [LocalizedDescriptionAttribute("EffectFollowParentBehavior_SingleInstanceID")]
        public string SingleInstanceID { get; set; }

        [XmlElement]
        [LocalizedDescriptionAttribute("EffectFollowParentBehavior_FailIfParentMotionChanged")]
        public bool FailIfParentMotionChanged { get; set; }

        [XmlElement]
        [LocalizedDescriptionAttribute("EffectFollowParentBehavior_ReleaseIfCheckFailed")]
        public bool ReleaseIfCheckFailed { get; set; }

        [XmlElement]
        [LocalizedDescriptionAttribute("EffectFollowParentBehavior_SegmentCheckFailed")]
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
    [LocalizedClassDisplayName(typeof(ClearFollowingEffectBehavior))]
    public class ClearFollowingEffectBehavior : Behavior
    {
        [XmlElement]
        [EditorChildNode("ClearFollowingEffectBehavior_Time")]
        public Time Time;

        [XmlElement]
        [LocalizedDescriptionAttribute("ClearFollowingEffectBehavior_SingleInstanceID")]
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
