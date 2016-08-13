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
    public class EffectSegmentBehaviorEntryGenEnv
    {
        public ActionEffects Dest;
        public int Segment;
        public string VariablePrefix;
        public Effect FinishEffect;
    }

    [Serializable]
    [SerializationBaseClass]
    public abstract class EffectSegmentBehaviorEntry
    {
        public abstract void MakeEffects(EffectSegmentBehaviorEntryGenEnv env);
    }

    [Serializable]
    [DisplayName("ScaleRatio")]
    public class EffectActorBehaviorEntryReduceScaleRatio : EffectSegmentBehaviorEntry
    {
        [XmlElement]
        public float RatioX { get; set; }

        [XmlElement]
        public float RatioY { get; set; }

        public override void MakeEffects(EffectSegmentBehaviorEntryGenEnv env)
        {
            var rx = new ConstValue { Value = RatioX };
            var ry = new ConstValue { Value = RatioY };
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
            env.Dest.UpdateEffects.Add(new FilteredEffect
            {
                Filter = new Effects.AnimationSegmentFilter { Segment = env.Segment },
                Effect = e,
            });
        }
    }

    [Serializable]
    [DisplayName("FinishAfter")]
    public class EffectActorBehaviorEntryFinisheAfter : EffectSegmentBehaviorEntry
    {
        [XmlAttribute]
        public int Tick { get; set; }

        public override void MakeEffects(EffectSegmentBehaviorEntryGenEnv env)
        {
            var varName = env.VariablePrefix + "_segment_start_tick_" + env.Segment;
            env.Dest.SegmentStartEffects.AddEffectToList(env.Segment,
                new Effects.ActorSetFloatVariableEffect
                {
                    Name = varName,
                    Value = new Effects.ActorMemberValue { Type = ActorMemberType.count },
                });
            var effect = new FilteredEffect
            {
                Filter = new AnimationCountAfterFilter
                {
                    Count = new BinaryExpressionValue
                    {
                        Left = new ConstValue { Value = Tick },
                        Right = new Effects.ActorFloatVariableValue { Name = varName },
                        Operator = BinaryOperator.Add,
                    },
                },
                Effect = env.FinishEffect,
            };
            env.Dest.UpdateEffects.Add(new FilteredEffect
            {
                Filter = new AnimationSegmentFilter { Segment = env.Segment },
                Effect = effect,
            });
        }
    }

    [Serializable]
    [DisplayName("ReduceAlpha")]
    public class EffectActorBehaviorEntryReduceAlpha : EffectSegmentBehaviorEntry
    {
        [XmlElement]
        [DefaultValue(1.0f)]
        public float Multiply { get; set; }

        [XmlElement]
        [DefaultValue(0.0f)]
        public float Minus { get; set; }

        public EffectActorBehaviorEntryReduceAlpha()
        {
            Multiply = 1.0f;
            Minus = 0.0f;
        }

        public override void MakeEffects(EffectSegmentBehaviorEntryGenEnv env)
        {
            var effect = new SimpleListEffect();
            effect.EffectList.Add(new Effects.SetActorMemberEffect
            {
                Type = ActorMemberType.alpha,
                Value = new BinaryExpressionValue
                {
                    Left = new ActorMemberValue { Type = ActorMemberType.alpha },
                    Right = new ConstValue { Value = Multiply },
                    Operator = BinaryOperator.Multiply,
                },
            });
            effect.EffectList.Add(new Effects.SetActorMemberEffect
            {
                Type = ActorMemberType.alpha,
                Value = new BinaryExpressionValue
                {
                    Left = new ActorMemberValue { Type = ActorMemberType.alpha },
                    Right = new ConstValue { Value = Minus },
                    Operator = BinaryOperator.Minus,
                },
            });

            var finishEffect = new SimpleListEffect();
            finishEffect.EffectList.Add(new SetActorMemberEffect
            {
                Type = ActorMemberType.alpha,
                Value = new ConstValue { Value = 0.0f },
            });
            finishEffect.EffectList.Add(env.FinishEffect);

            effect.EffectList.Add(new FilteredEffect
            {
                Filter = new ValueCompareFilter
                {
                    Operator = CompareOperator.GreaterOrEqual,
                    Left = new ConstValue { Value = 0.0f },
                    Right = new ActorMemberValue { Type = ActorMemberType.alpha },
                },
                Effect = finishEffect,
            });

            env.Dest.UpdateEffects.Add(new FilteredEffect
            {
                Filter = new AnimationSegmentFilter { Segment = env.Segment },
                Effect = effect,
            });
        }
    }

    [Serializable]
    [DisplayName("IncreaseAlpha")]
    public class EffectActorBehaviorEntryIncreaseAlpha : EffectSegmentBehaviorEntry
    {
        [XmlElement]
        public float IncreaseFrom { get; set; }

        [XmlElement]
        [DefaultValue(0.0f)]
        public float Increase { get; set; }

        public EffectActorBehaviorEntryIncreaseAlpha()
        {
            Increase = 0.0f;
        }

        public override void MakeEffects(EffectSegmentBehaviorEntryGenEnv env)
        {
            env.Dest.SegmentStartEffects.AddEffectToList(env.Segment, new SetActorMemberEffect
            {
                Type = ActorMemberType.alpha,
                Value = new ConstValue { Value = IncreaseFrom },
            });

            var effect = new SimpleListEffect();
            effect.EffectList.Add(new Effects.SetActorMemberEffect
            {
                Type = ActorMemberType.alpha,
                Value = new BinaryExpressionValue
                {
                    Left = new ActorMemberValue { Type = ActorMemberType.alpha },
                    Right = new ConstValue { Value = Increase },
                    Operator = BinaryOperator.Add,
                },
            });

            var finishEffect = new SimpleListEffect();
            finishEffect.EffectList.Add(new SetActorMemberEffect
            {
                Type = ActorMemberType.alpha,
                Value = new ConstValue { Value = 1.0f },
            });
            finishEffect.EffectList.Add(env.FinishEffect);

            effect.EffectList.Add(new FilteredEffect
            {
                Filter = new ValueCompareFilter
                {
                    Operator = CompareOperator.GreaterOrEqual,
                    Left = new ActorMemberValue { Type = ActorMemberType.alpha },
                    Right = new ConstValue { Value = 1.0f },
                },
                Effect = finishEffect,
            });

            env.Dest.UpdateEffects.Add(new FilteredEffect
            {
                Filter = new AnimationSegmentFilter { Segment = env.Segment },
                Effect = effect,
            });
        }
    }

    [Serializable]
    [DisplayName("Flash")]
    public class EffectActorBehaviorEntryFlash : EffectSegmentBehaviorEntry
    {
        [XmlElement]
        public float Max { get; set; }

        [XmlElement]
        public float Min { get; set; }

        [XmlElement]
        public float Step { get; set; }

        public override void MakeEffects(EffectSegmentBehaviorEntryGenEnv env)
        {
            var varname = env.VariablePrefix + "flash_inc";

            env.Dest.SegmentStartEffects.AddEffectToList(env.Segment, new ActorSetFloatVariableEffect
            {
                Name = varname,
                Value = new ConstValue { Value = -1.0f },
            });

            var inc = new SetActorMemberEffect
            {
                Type = ActorMemberType.alpha,
                Value = new BinaryExpressionValue
                {
                    Left = new ActorMemberValue { Type = ActorMemberType.alpha },
                    Right = new ConstValue { Value = Step },
                    Operator = BinaryOperator.Add,
                },
            };
            var dec = new SetActorMemberEffect
            {
                Type = ActorMemberType.alpha,
                Value = new BinaryExpressionValue
                {
                    Left = new ActorMemberValue { Type = ActorMemberType.alpha },
                    Right = new ConstValue { Value = Step },
                    Operator = BinaryOperator.Minus,
                },
            };

            var setmax = new SetActorMemberEffect
            {
                Type = ActorMemberType.alpha,
                Value = new ConstValue { Value = Max },
            };
            var setmin = new SetActorMemberEffect
            {
                Type = ActorMemberType.alpha,
                Value = new ConstValue { Value = Min },
            };

            var checkInc = new ValueCompareFilter
            {
                Left = new ActorFloatVariableValue { Name = varname },
                Right = new ConstValue { Value = 0.0f },
                Operator = CompareOperator.Greater,
            };
            var checkDec = new ValueCompareFilter
            {
                Left = new ConstValue { Value = 0.0f },
                Right = new ActorFloatVariableValue { Name = varname },
                Operator = CompareOperator.Greater,
            };

            var checkMax = new ValueCompareFilter
            {
                Left = new ActorMemberValue { Type = ActorMemberType.alpha },
                Right = new ConstValue { Value = Max },
                Operator = CompareOperator.GreaterOrEqual,
            };
            var checkMin = new ValueCompareFilter
            {
                Left = new ConstValue { Value = Min },
                Right = new ActorMemberValue { Type = ActorMemberType.alpha },
                Operator = CompareOperator.GreaterOrEqual,
            };

            var turnDec = new SimpleListEffect();
            turnDec.EffectList.Add(setmax);
            turnDec.EffectList.Add(new ActorSetFloatVariableEffect
            {
                Name = varname,
                Value = new ConstValue { Value = -1.0f },
            });
            var turnInc = new SimpleListEffect();
            turnInc.EffectList.Add(setmin);
            turnInc.EffectList.Add(new ActorSetFloatVariableEffect
            {
                Name = varname,
                Value = new ConstValue { Value = 1.0f },
            });

            var effectInc = new SimpleListEffect();
            effectInc.EffectList.Add(inc);
            effectInc.EffectList.Add(new FilteredEffect
            {
                Filter = checkMax,
                Effect = turnDec,
            });

            var effectDec = new SimpleListEffect();
            effectDec.EffectList.Add(dec);
            effectDec.EffectList.Add(new FilteredEffect
            {
                Filter = checkMin,
                Effect = turnInc,
            });

            var effect = new SimpleListEffect();
            effect.EffectList.Add(new FilteredEffect
            {
                Filter = checkInc,
                Effect = effectInc,
            });
            effect.EffectList.Add(new FilteredEffect
            {
                Filter = checkDec,
                Effect = effectDec,
            });

            env.Dest.UpdateEffects.Add(new FilteredEffect
            {
                Filter = new AnimationSegmentFilter { Segment = env.Segment },
                Effect = effect,
            });
        }
    }

    [Serializable]
    [DisplayName("InitRandomScale")]
    public class EffectActorBehaviorEntryInitRandomScale : EffectSegmentBehaviorEntry
    {
        [XmlAttribute]
        public float MaxX { get; set; }

        [XmlAttribute]
        public float MinX { get; set; }

        [XmlAttribute]
        public float MaxY { get; set; }

        [XmlAttribute]
        public float MinY { get; set; }

        public override void MakeEffects(EffectSegmentBehaviorEntryGenEnv env)
        {
            Value valX = (MaxX == MinX) ?
                (Value)new ConstValue { Value = MaxX } : new RandomFloatValue
                {
                    Max = MaxX,
                    Min = MinX,
                    Step = (MaxX - MinX) / 100,
                };
            Value valY = (MaxY == MinY) ?
                (Value)new ConstValue { Value = MaxY } : new RandomFloatValue
                {
                    Max = MaxY,
                    Min = MinY,
                    Step = (MaxY - MinY) / 100,
                };
            var e = new SimpleListEffect();
            e.EffectList.Add(new SetActorMemberEffect
            {
                Type = ActorMemberType.sx,
                Value = valX,
            });
            e.EffectList.Add(new SetActorMemberEffect
            {
                Type = ActorMemberType.sy,
                Value = valY,
            });
            env.Dest.SegmentStartEffects.AddEffectToList(env.Segment, e);
        }
    }

    [Serializable]
    [DisplayName("InitRandomRotation")]
    public class EffectActorBehaviorEntryInitRandomRotation : EffectSegmentBehaviorEntry
    {
        [XmlAttribute]
        public float Max { get; set; }

        [XmlAttribute]
        public float Min { get; set; }

        public override void MakeEffects(EffectSegmentBehaviorEntryGenEnv env)
        {
            Value val = (Max == Min) ?
                (Value)new ConstValue { Value = Max } : new RandomFloatValue
                {
                    Max = Max,
                    Min = Min,
                    Step = (Max - Min) / 100,
                };

            env.Dest.SegmentStartEffects.AddEffectToList(env.Segment, new SetActorMemberEffect
            {
                Type = ActorMemberType.rz,
                Value = val,
            });
        }
    }

    [Serializable]
    [DisplayName("RandomReverseY")]
    public class EffectActorBehaviorEntryRandomReverse : EffectSegmentBehaviorEntry
    {
        public override void MakeEffects(EffectSegmentBehaviorEntryGenEnv env)
        {
            env.Dest.SegmentStartEffects.AddEffectToList(env.Segment, new FilteredEffect
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
    public class EffectSegmentBehavior : Behavior
    {
        [XmlArray]
        [EditorChildNode(null)]
        public List<EffectSegmentBehaviorEntry> Entries = new List<EffectSegmentBehaviorEntry>();

        [XmlAttribute]
        public int Segment { get; set; }

        [XmlElement]
        public int? FinishSegment { get; set; }

        [XmlElement]
        public bool ReleaseWhenFinish { get; set; }

        public override void MakeEffects(ActionEffects effects)
        {
            var effect = new SimpleListEffect();
            if (FinishSegment.HasValue)
            {
                effect.EffectList.Add(new Effects.SetSegmentEffect { Segment = FinishSegment.Value });
            }
            if (ReleaseWhenFinish)
            {
                effect.EffectList.Add(new Effects.ReleaseActorEffect());
            }
            var env = new EffectSegmentBehaviorEntryGenEnv
            {
                Dest = effects,
                Segment = Segment,
                VariablePrefix = this.GetVariableName("entry_"),
                FinishEffect = effect,
            };
            foreach (var e in Entries)
            {
                e.MakeEffects(env);
            }
        }
    }
}
