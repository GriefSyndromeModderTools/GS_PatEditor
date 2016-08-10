using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat.Behaviors
{
    [Serializable]
    public class HoldSkillBehavior : Behavior
    {
        [XmlElement]
        public int HoldSegment { get; set; }
        [XmlElement]
        public int SuccessSegment { get; set; }
        [XmlElement]
        public int CancelSegment { get; set; }
        [XmlElement]
        public int HoldLength { get; set; }

        public override void MakeEffects(ActionEffects effects)
        {
            var variableName = this.GetVariableName("hold_end_tick");
            effects.SegmentStartEffects.AddEffectToList(HoldSegment,
                new Effects.ActorSetFloatVariableEffect
                {
                    Name = variableName,
                    Value = new Effects.BinaryExpressionValue
                    {
                        Operator = Effects.BinaryOperator.Add,
                        Left = new Effects.ActorMemberValue { Type = Effects.ActorMemberType.count },
                        Right = new ConstValue { Value = HoldLength },
                    },
                });
            //cancel check
            effects.UpdateEffects.Add(new FilteredEffect
            {
                Filter = new SimpleListFilter(
                    new Effects.AnimationSegmentFilter { Segment = HoldSegment },
                    new Effects.ValueCompareFilter
                    {
                        Operator = Effects.CompareOperator.Greater,
                        Right = new Effects.ActorMemberValue { Type = Effects.ActorMemberType.count },
                        Left = new Effects.ActorFloatVariableValue { Name = variableName },
                    },
                    new Effects.PlayerKeyReleasedFilter()),
                Effect = new Effects.SetSegmentEffect { Segment = CancelSegment },
            });
            //success check
            effects.UpdateEffects.Add(new FilteredEffect
            {
                Filter = new SimpleListFilter(
                    new Effects.AnimationSegmentFilter { Segment = HoldSegment },
                    new Effects.ValueCompareFilter
                    {
                        Operator = Effects.CompareOperator.GreaterOrEqual,
                        Left = new Effects.ActorMemberValue { Type = Effects.ActorMemberType.count },
                        Right = new Effects.ActorFloatVariableValue { Name = variableName },
                    }),
                Effect = new Effects.SetSegmentEffect { Segment = SuccessSegment },
            });
        }
    }
}
