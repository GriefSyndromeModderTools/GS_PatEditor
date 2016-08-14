using GS_PatEditor.Editor.Editable;
using GS_PatEditor.Pat.Effects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat.Behaviors
{
    [Serializable]
    [SerializationBaseClassAttribute]
    public abstract class PlayerGroundSpeedCtrlBehaviorEntry
    {
        public abstract void MakeEffects(ActionEffects effects);
    }

    [Serializable]
    [LocalizedClassDisplayName(typeof(PlayerGroundSpeedCtrlBehaviorEntryFriction))]
    public class PlayerGroundSpeedCtrlBehaviorEntryFriction : PlayerGroundSpeedCtrlBehaviorEntry
    {
        [XmlAttribute]
        [DefaultValue(0.2f)]
        [LocalizedDescriptionAttribute("PlayerGroundSpeedCtrlBehaviorEntryFriction_Value")]
        public float Value { get; set; }

        [XmlElement]
        public SegmentSelector Segments { get; set; }
        public bool ShouldSerializeSegments()
        {
            return !(Segments != null && Segments.Index == "*");
        }

        public PlayerGroundSpeedCtrlBehaviorEntryFriction()
        {
            Value = 0.2f;
            Segments = new SegmentSelector { Index = "*" };
        }

        private Effect GetEffect()
        {
            return new Effects.PlayerSkillStopMovingEffect { ReduceSpeed = Value };
        }

        public override void MakeEffects(ActionEffects effects)
        {
            SegmentSelectorHelper.MakeEffectsAsUpdate(effects, Segments, GetEffect());
        }
    }

    [Serializable]
    [LocalizedClassDisplayName(typeof(PlayerGroundSpeedCtrlBehaviorEntryRecoil))]
    public class PlayerGroundSpeedCtrlBehaviorEntryRecoil : PlayerGroundSpeedCtrlBehaviorEntry
    {
        [XmlElement]
        [EditorChildNode("PlayerGroundSpeedCtrlBehaviorEntryRecoil_Time")]
        public Time Time;

        [XmlElement]
        [DefaultValue(null)]
        [LocalizedDescriptionAttribute("PlayerGroundSpeedCtrlBehaviorEntryRecoil_CheckBefore")]
        public float? CheckBefore { get; set; }

        [XmlElement]
        [DefaultValue(false)]
        [LocalizedDescriptionAttribute("PlayerGroundSpeedCtrlBehaviorEntryRecoil_StrictCheckBefore")]
        public bool StrictCheckBefore { get; set; }

        [XmlElement]
        [LocalizedDescriptionAttribute("PlayerGroundSpeedCtrlBehaviorEntryRecoil_Value")]
        public float Value { get; set; }

        [XmlElement]
        [DefaultValue(null)]
        [LocalizedDescriptionAttribute("PlayerGroundSpeedCtrlBehaviorEntryRecoil_CheckAfter")]
        public float? CheckAfter { get; set; }

        public override void MakeEffects(ActionEffects effects)
        {
            if (Value == 0)
            {
                return;
            }
            var e = MakeValue();
            if (CheckAfter.HasValue)
            {
                var sle = new SimpleListEffect();
                sle.EffectList.Add(e);
                sle.EffectList.Add(MakeCheckAfter());
                e = sle;
            }
            if (CheckBefore.HasValue)
            {
                e = MakeCheckBefore(e);
            }
            Time.MakeEffects(effects, e);
        }

        private Effect MakeCheckBefore(Effect e)
        {
            var cmp = StrictCheckBefore ? CompareOperator.Greater : CompareOperator.GreaterOrEqual;
            var cv = new ConstValue { Value = CheckBefore.Value };
            var pv = new Effects.ActorMemberValue { Type = ActorMemberType.vx };
            if (Value > 0)
            {
                return new FilteredEffect
                {
                    Filter = new Effects.ValueCompareFilter
                    {
                        Operator = cmp,
                        Left = cv,
                        Right = pv,
                    },
                    Effect = e,
                };
            }
            else
            {
                return new FilteredEffect
                {
                    Filter = new Effects.ValueCompareFilter
                    {
                        Operator = cmp,
                        Left = pv,
                        Right = cv,
                    },
                    Effect = e,
                };
            }
        }

        private Effect MakeValue()
        {
            return new Effects.SetActorMemberEffect
            {
                Type = ActorMemberType.vx,
                Value = new BinaryExpressionValue
                {
                    Operator = BinaryOperator.Add,
                    Left = new ActorMemberValue { Type = ActorMemberType.vx },
                    Right = new ConstValue { Value = Value },
                },
            };
        }

        private Effect MakeCheckAfter()
        {
            var cv = new ConstValue { Value = CheckAfter.Value };
            var pv = new Effects.ActorMemberValue { Type = ActorMemberType.vx };
            var e = new Effects.SetActorMemberEffect
            {
                Type = ActorMemberType.vx,
                Value = cv,
            };
            if (Value > 0)
            {
                return new FilteredEffect
                {
                    Filter = new Effects.ValueCompareFilter
                    {
                        Operator = CompareOperator.Greater,
                        Left = pv,
                        Right = cv,
                    },
                    Effect = e,
                };
            }
            else
            {
                return new FilteredEffect
                {
                    Filter = new Effects.ValueCompareFilter
                    {
                        Operator = CompareOperator.Greater,
                        Left = cv,
                        Right = pv,
                    },
                    Effect = e,
                };
            }
        }
    }

    [Serializable]
    [LocalizedClassDisplayName(typeof(PlayerGroundSpeedCtrlBehavior))]
    public class PlayerGroundSpeedCtrlBehavior : Behavior
    {
        [XmlElement]
        [LocalizedDescriptionAttribute("PlayerGroundSpeedCtrlBehaviorEntryRecoil_ReduceInitialSpeed")]
        public float? ReduceInitialSpeed { get; set; }
        public bool ShouldSerializeReduceInitialSpeed()
        {
            return !(ReduceInitialSpeed.HasValue && ReduceInitialSpeed.Value == 0.25f);
        }

        [XmlArray]
        [EditorChildNode(null)]
        public List<PlayerGroundSpeedCtrlBehaviorEntry> Entries = new List<PlayerGroundSpeedCtrlBehaviorEntry>();

        public PlayerGroundSpeedCtrlBehavior()
        {
            ReduceInitialSpeed = 0.25f;
        }

        public override void MakeEffects(ActionEffects effects)
        {
            if (ReduceInitialSpeed.HasValue)
            {
                effects.InitEffects.Add(new SetActorMemberEffect
                {
                    Type = ActorMemberType.vx,
                    Value = new BinaryExpressionValue
                    {
                        Operator = BinaryOperator.Multiply,
                        Left = new ActorMemberValue { Type = ActorMemberType.vx },
                        Right = new ConstValue { Value = ReduceInitialSpeed.Value },
                    },
                });
            }
            foreach (var e in Entries)
            {
                e.MakeEffects(effects);
            }
        }
    }
}
