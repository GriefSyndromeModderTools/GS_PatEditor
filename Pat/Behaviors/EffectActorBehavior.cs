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
    [SerializationBaseClassAttribute]
    public abstract class EffectActorBehaviorEntry
    {
        public abstract void MakeEffects(ActionEffects effects);
    }

    [Serializable]
    [DisplayName("InitRandomScale")]
    public class EffectActorBehaviorEntryInitRandomScale : EffectActorBehaviorEntry
    {
        [XmlAttribute]
        public float Max { get; set; }
        [XmlAttribute]
        public float Min { get; set; }

        public override void MakeEffects(ActionEffects effects)
        {
            var val = new RandomFloatValue
            {
                Max = Max,
                Min = Min,
                Step = (Max - Min) / 100,
            };
            var e = new SimpleListEffect();
            e.EffectList.Add(new SetActorMemberEffect
            {
                Type = ActorMemberType.sx,
                Value = val,
            });
            e.EffectList.Add(new SetActorMemberEffect
            {
                Type = ActorMemberType.sy,
                Value = new ActorMemberValue
                {
                    Type = ActorMemberType.sx,
                },
            });
            effects.InitEffects.Add(e);
        }
    }
    
    [Serializable]
    [DisplayName("ReduceScaleRatio")]
    public class EffectActorBehaviorEntryReduceScaleRatio : EffectActorBehaviorEntry
    {
        [XmlElement]
        public float RatioX { get; set; }

        [XmlElement]
        public float? RatioY { get; set; }
        
        public override void MakeEffects(ActionEffects effects)
        {
            var rx = new ConstValue { Value = RatioX };
            var ry = RatioY.HasValue ? new ConstValue { Value = RatioY.Value } : rx;
            var e = new SimpleListEffect();
            e.EffectList.Add(new SetActorMemberEffect
            {
                Type = ActorMemberType.sx,
                Value = new BinaryExpressionValue
                {
                    Operator = BinaryOperator.Multiply,
                    Left = new ActorMemberValue { Type = ActorMemberType.sx },
                    Right = rx,
                },
            });
            e.EffectList.Add(new SetActorMemberEffect
            {
                Type = ActorMemberType.sy,
                Value = new BinaryExpressionValue
                {
                    Operator = BinaryOperator.Multiply,
                    Left = new ActorMemberValue { Type = ActorMemberType.sy },
                    Right = ry,
                },
            });
            effects.UpdateEffects.Add(e);
        }
    }

    [Serializable]
    [DisplayName("ReleaseAfter")]
    public class EffectActorBehaviorEntryReleaseAfter : EffectActorBehaviorEntry
    {
        [XmlAttribute]
        public int Tick { get; set; }

        public override void MakeEffects(ActionEffects effects)
        {
            effects.UpdateEffects.Add(new FilteredEffect
            {
                Filter = new AnimationCountAfterFilter { Count = new ConstValue { Value = Tick } },
                Effect = new ReleaseActorEffect(),
            });
        }
    }

    [Serializable]
    [DisplayName("RandomReverseY")]
    public class EffectActorBehaviorEntryRandomReverse : EffectActorBehaviorEntry
    {
        public override void MakeEffects(ActionEffects effects)
        {
            effects.InitEffects.Add(new FilteredEffect
            {
                Filter = new ValueCompareFilter
                {
                    Operator = CompareOperator.GreaterOrEqual,
                    Left = new RandomFloatValue { Max = 1, Min = 0, Step = 0.001f },
                    Right = new ConstValue { Value = 0.5f },
                },
                Effect = new SetActorMemberEffect
                {
                    Type = ActorMemberType.sy,
                    Value = new BinaryExpressionValue
                    {
                        Operator = BinaryOperator.Multiply,
                        Left = new ActorMemberValue { Type = ActorMemberType.sy },
                        Right = new ConstValue { Value = -1 },
                    },
                },
            });
        }
    }

    [Serializable]
    public class EffectActorBehavior : Behavior
    {
        [XmlArray]
        [EditorChildNode(null)]
        public List<EffectActorBehaviorEntry> Entries = new List<EffectActorBehaviorEntry>();

        public override void MakeEffects(ActionEffects effects)
        {
            effects.InitEffects.Add(new BulletInitEffect());
            effects.InitEffects.Add(new InitCountEffect());
            effects.UpdateEffects.Add(new IncreaseCountEffect());
            effects.SegmentFinishEffects.AddEffectToList(effects.SegmentCount, new ReleaseActorEffect());
            foreach (var e in Entries)
            {
                e.MakeEffects(effects);
            }
        }
    }
}
