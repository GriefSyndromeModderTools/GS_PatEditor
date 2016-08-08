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
    public abstract class PlayerAirSpeedCtrlBehaviorEntry
    {
        public abstract void MakeEffects(ActionEffects effects);
    }

    [Serializable]
    [DisplayName("Gravity")]
    public class PlayerAirSpeedCtrlBehaviorEntryGravity : PlayerAirSpeedCtrlBehaviorEntry
    {
        [XmlElement]
        [DefaultValue(1.0f)]
        public float Value { get; set; }

        [XmlElement]
        public SegmentSelector Segments { get; set; }

        public PlayerAirSpeedCtrlBehaviorEntryGravity()
        {
            Value = 1.0f;
            Segments = new SegmentSelector { Index = "*" };
        }

        public override void MakeEffects(ActionEffects effects)
        {
            SegmentSelectorHelper.MakeEffectsAsUpdate(effects, Segments,
                new GravityEffect { Value = Value });
        }
    }

    [Serializable]
    [DisplayName("ReduceSpeed")]
    public class PlayerAirSpeedCtrlBehaviorEntryReduceSpeed : PlayerAirSpeedCtrlBehaviorEntry
    {
        [XmlElement]
        public float? RatioX { get; set; }

        [XmlElement]
        public float? RatioY { get; set; }

        [XmlElement]
        [EditorChildNode("Time")]
        public Time Time;

        private Effect GetEffect()
        {
            var list = new SimpleListEffect();
            if (RatioX.HasValue)
            {
                list.EffectList.Add(new SetActorMemberEffect
                {
                    Type = ActorMemberType.vx,
                    Value = new BinaryExpressionValue
                    {
                        Operator = BinaryOperator.Multiply,
                        Left = new ActorMemberValue { Type = ActorMemberType.vx },
                        Right = new ConstValue { Value = RatioX.Value },
                    },
                });
            }
            if (RatioY.HasValue)
            {
                list.EffectList.Add(new SetActorMemberEffect
                {
                    Type = ActorMemberType.vy,
                    Value = new BinaryExpressionValue
                    {
                        Operator = BinaryOperator.Multiply,
                        Left = new ActorMemberValue { Type = ActorMemberType.vy },
                        Right = new ConstValue { Value = RatioY.Value },
                    },
                });
            }
            return list;
        }

        public override void MakeEffects(ActionEffects effects)
        {
            Time.MakeEffects(effects, GetEffect());
        }
    }

    [Serializable]
    [DisplayName("RecoilX")]
    public class PlayerAirSpeedCtrlBehaviorEntryRecoil : PlayerAirSpeedCtrlBehaviorEntry
    {
        [XmlElement]
        [EditorChildNode("Time")]
        public Time Time;

        [XmlElement]
        [DefaultValue(null)]
        public float? CheckBefore { get; set; }

        [XmlElement]
        [DefaultValue(false)]
        public bool StrictCheckBefore { get; set; }

        [XmlElement]
        public float Value { get; set; }

        [XmlElement]
        [DefaultValue(null)]
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
    [DisplayName("AirJump")]
    public class PlayerAirSpeedCtrlBehaviorEntryAirJump : PlayerAirSpeedCtrlBehaviorEntry
    {
        [XmlAttribute]
        public float Speed { get; set; }

        [XmlAttribute]
        [DefaultValue(true)]
        public bool ResetOriginSpeed { get; set; }

        [XmlElement]
        [EditorChildNode("Time")]
        public Time Time;

        public PlayerAirSpeedCtrlBehaviorEntryAirJump()
        {
            ResetOriginSpeed = true;
        }

        public override void MakeEffects(ActionEffects effects)
        {
            Value v = new ConstValue { Value = -Speed };
            if (!ResetOriginSpeed)
            {
                v = new BinaryExpressionValue
                {
                    Left = v,
                    Right = new ActorMemberValue { Type = ActorMemberType.vy },
                    Operator = BinaryOperator.Add,
                };
            }
            var effect = new SetActorMemberEffect
            {
                Type = ActorMemberType.vy,
                Value = v,
            };
            Time.MakeEffects(effects, effect);
        }
    }

    [Serializable]
    public class PlayerAirSpeedCtrlBehavior : Behavior
    {
        [XmlElement]
        public float? ReduceInitialSpeedX { get; set; }

        [XmlElement]
        public float? ReduceInitialSpeedY { get; set; }

        [XmlElement]
        public float? InitialGravity { get; set; }

        [XmlArray]
        [EditorChildNode(null)]
        public List<PlayerAirSpeedCtrlBehaviorEntry> Entries = new List<PlayerAirSpeedCtrlBehaviorEntry>();

        public override void MakeEffects(ActionEffects effects)
        {
            if (ReduceInitialSpeedX.HasValue)
            {
                effects.InitEffects.Add(new SetActorMemberEffect
                {
                    Type = ActorMemberType.vx,
                    Value = new BinaryExpressionValue
                    {
                        Operator = BinaryOperator.Multiply,
                        Left = new ActorMemberValue { Type = ActorMemberType.vx },
                        Right = new ConstValue { Value = ReduceInitialSpeedX.Value },
                    },
                });
            }
            if (ReduceInitialSpeedY.HasValue)
            {
                effects.InitEffects.Add(new SetActorMemberEffect
                {
                    Type = ActorMemberType.vy,
                    Value = new BinaryExpressionValue
                    {
                        Operator = BinaryOperator.Multiply,
                        Left = new ActorMemberValue { Type = ActorMemberType.vy },
                        Right = new ConstValue { Value = ReduceInitialSpeedY.Value },
                    },
                });
            }
            if (InitialGravity.HasValue)
            {
                effects.InitEffects.Add(new GravityEffect
                {
                    Value = InitialGravity.Value,
                });
            }
            foreach (var e in Entries)
            {
                e.MakeEffects(effects);
            }
        }
    }
}
