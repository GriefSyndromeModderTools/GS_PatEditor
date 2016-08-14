using GS_PatEditor.Editor.Editable;
using GS_PatEditor.Editor.Exporters;
using GS_PatEditor.Editor.Exporters.CodeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat.Effects
{
    [Serializable]
    public class AnimationSegmentFilter : Filter
    {
        [XmlAttribute]
        public int Segment { get; set; }

        public override bool Test(Simulation.Actor actor)
        {
            return actor.CurrentSegmentIndex == Segment;
        }

        public override Expression Generate(GenerationEnvironment env)
        {
            return new BiOpExpr(ThisExpr.Instance.MakeIndex("keyTake"), new ConstNumberExpr(Segment), BiOpExpr.Op.Equal);
        }
    }

    [Serializable]
    public class AnimationCountAfterFilter : Filter
    {
        [XmlElement]
        [EditorChildNode("AnimationCountAfterFilter_Count")]
        public Value Count;

        public override bool Test(Simulation.Actor actor)
        {
            return actor.ActionCount > Count.GetInt(actor);
        }

        public override Expression Generate(GenerationEnvironment env)
        {
            return new BiOpExpr(ThisExpr.Instance.MakeIndex("count"), Count.Generate(env), BiOpExpr.Op.Greater);
        }
    }

    [Serializable]
    public class PlayerKeyReleasedFilter : Filter
    {
        //TODO allow control in editor
        public override bool Test(Simulation.Actor actor)
        {
            return false;
        }

        public override Expression Generate(GenerationEnvironment env)
        {
            return ThisExpr.Instance.MakeIndex("input").MakeIndex(env.GetCurrentSkillKeyName()).IsZero();
        }
    }

    [Serializable]
    public class AnimationCountModFilter : Filter
    {
        [XmlElement]
        [EditorChildNode("AnimationCountModFilter_Divisor")]
        public Value Divisor;
        [XmlElement]
        [EditorChildNode("AnimationCountModFilter_Mod")]
        public Value Mod;

        public override bool Test(Simulation.Actor actor)
        {
            return (actor.ActionCount % Divisor.GetInt(actor)) == Mod.GetInt(actor);
        }

        public override Expression Generate(GenerationEnvironment env)
        {
            var count = ThisExpr.Instance.MakeIndex("count");
            return new BiOpExpr(
                new BiOpExpr(count, Divisor.Generate(env), BiOpExpr.Op.Mod),
                Mod.Generate(env), BiOpExpr.Op.Equal);
        }
    }

    public enum CompareOperator
    {
        Equal,
        Greater,
        GreaterOrEqual,
        NotEqual,
    }

    [Serializable]
    public class ValueCompareFilter : Filter
    {
        [XmlElement]
        [EditorChildNode("ValueCompareFilter_Left")]
        public Value Left;

        [XmlElement]
        [EditorChildNode("ValueCompareFilter_Right")]
        public Value Right;

        [XmlElement]
        public CompareOperator Operator { get; set; }

        public override bool Test(Simulation.Actor actor)
        {
            switch (Operator)
            {
                case CompareOperator.Equal:
                    return Left.Get(actor) == Right.Get(actor);
                case CompareOperator.Greater:
                    return Left.Get(actor) > Right.Get(actor);
                case CompareOperator.GreaterOrEqual:
                    return Left.Get(actor) >= Right.Get(actor);
                case CompareOperator.NotEqual:
                    return Left.Get(actor) != Right.Get(actor);
                default:
                    return false;
            }
        }

        public override Expression Generate(GenerationEnvironment env)
        {
            BiOpExpr.Op opr;
            switch (Operator)
            {
                case CompareOperator.Equal:
                    opr = BiOpExpr.Op.Equal;
                    break;
                case CompareOperator.Greater:
                    opr = BiOpExpr.Op.Greater;
                    break;
                case CompareOperator.GreaterOrEqual:
                    opr = BiOpExpr.Op.GreaterOrEqual;
                    break;
                case CompareOperator.NotEqual:
                    opr = BiOpExpr.Op.NotEqual;
                    break;
                default:
                    return new ConstNumberExpr(0);
            }
            return new BiOpExpr(Left.Generate(env), Right.Generate(env), opr);
        }


    }
}
