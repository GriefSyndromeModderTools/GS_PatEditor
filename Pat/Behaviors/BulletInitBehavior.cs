using GS_PatEditor.Editor.Editable;
using GS_PatEditor.Editor.Exporters;
using GS_PatEditor.Editor.Exporters.CodeFormat;
using GS_PatEditor.Pat.Effects;
using GS_PatEditor.Simulation;
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
    [LocalizedClassDisplayName(typeof(BulletInitBehavior))]
    public class BulletInitBehavior : Behavior
    {
        [XmlAttribute]
        [DefaultValue(false)]
        [LocalizedDescriptionAttribute("BulletInitBehavior_IgnoreCollisionTransform")]
        public bool IgnoreCollisionTransform { get; set; }

        [XmlAttribute]
        [DefaultValue(false)]
        public bool ImmuneToTimeStop { get; set; }

        public override void MakeEffects(ActionEffects effects)
        {
            effects.InitEffects.Add(new BulletInitEffect());
            effects.InitEffects.Add(new InitCountEffect());
            if (!ImmuneToTimeStop) effects.UpdateEffects.Add(new TimeStopCheckEffect());
            effects.UpdateEffects.Add(new IncreaseCountEffect());
            effects.SegmentFinishEffects.AddEffectToList(effects.SegmentCount - 1,
                new Effects.ReleaseActorEffect());

            //implement auto collision update
            var vn_sx = this.GetVariableName("sx");
            var vn_sy = this.GetVariableName("sy");
            var vn_rz = this.GetVariableName("rz");

            //set init value
            var resetVars = new SimpleListEffect();
            resetVars.EffectList.Add(new ActorSetFloatVariableEffect
            {
                Name = vn_sx,
                Value = new ConstValue { Value = 1 },
            });
            resetVars.EffectList.Add(new ActorSetFloatVariableEffect
            {
                Name = vn_sy,
                Value = new ConstValue { Value = 1 },
            });
            resetVars.EffectList.Add(new ActorSetFloatVariableEffect
            {
                Name = vn_rz,
                Value = new ConstValue { Value = 0 },
            });
            effects.InitEffects.Add(resetVars);

            //check
            var cond1 = new ValueCompareFilter
            {
                Left = new ActorMemberValue { Type = ActorMemberType.sx },
                Right = new ActorFloatVariableValue { Name = vn_sx },
                Operator = CompareOperator.NotEqual,
            };
            var cond2 = new ValueCompareFilter
            {
                Left = new ActorMemberValue { Type = ActorMemberType.sy },
                Right = new ActorFloatVariableValue { Name = vn_sy },
                Operator = CompareOperator.NotEqual,
            };
            var cond3 = new ValueCompareFilter
            {
                Left = new ActorMemberValue { Type = ActorMemberType.rz },
                Right = new ActorFloatVariableValue { Name = vn_rz },
                Operator = CompareOperator.NotEqual,
            };

            var updateEffect = new SimpleListEffect();
            updateEffect.EffectList.Add(new BulletUpdateCollisionEffect());
            updateEffect.EffectList.Add(resetVars);

            //var checkEffect = new FilteredEffect
            //{
            //    Filter = new ValueCompareFilter
            //    {
            //        Left = cond,
            //        Right = new ConstValue { Value = 0 },
            //        Operator = CompareOperator.NotEqual,
            //    },
            //    Effect = updateEffect,
            //};
            var checkEffect = new SimpleListEffect();
            checkEffect.EffectList.Add(new FilteredEffect
            {
                Filter= cond1,
                Effect = updateEffect,
            });
            checkEffect.EffectList.Add(new FilteredEffect
            {
                Filter = cond2,
                Effect = updateEffect,
            });
            checkEffect.EffectList.Add(new FilteredEffect
            {
                Filter = cond3,
                Effect = updateEffect,
            });

            if (!IgnoreCollisionTransform)
            {
                effects.PostInitEffects.Add(checkEffect);
                effects.PostUpdateEffects.Effects.Add(checkEffect);
            }
        }
    }
}
