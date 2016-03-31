﻿using GS_PatEditor.Editor.Editable;
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
    public enum BinaryOperator
    {
        Add,
        Minus,
        Multiply,
        Divide,
    }

    [Serializable]
    public class BinaryExpressionValue : Value
    {
        [XmlElement]
        [EditorChildNode("Left")]
        public Value Left;

        [XmlElement]
        [EditorChildNode("Right")]
        public Value Right;

        [XmlAttribute]
        public BinaryOperator Operator { get; set; }

        public override float Get(Simulation.Actor actor)
        {
            switch (Operator)
            {
                case BinaryOperator.Add:
                    return Left.Get(actor) + Right.Get(actor);
                case BinaryOperator.Minus:
                    return Left.Get(actor) - Right.Get(actor);
                case BinaryOperator.Multiply:
                    return Left.Get(actor) * Right.Get(actor);
                case BinaryOperator.Divide:
                    return Left.Get(actor) / Right.Get(actor);
            }
            return 0.0f;
        }

        public override Expression Generate(GenerationEnvironment env)
        {
            BiOpExpr.Op opr = BiOpExpr.Op.Assign;
            switch (Operator)
            {
                case BinaryOperator.Add:
                    opr = BiOpExpr.Op.Add;
                    break;
                case BinaryOperator.Minus:
                    opr = BiOpExpr.Op.Minus;
                    break;
                case BinaryOperator.Multiply:
                    opr = BiOpExpr.Op.Multiply;
                    break;
                case BinaryOperator.Divide:
                    opr = BiOpExpr.Op.Divide;
                    break;
            }
            return new BiOpExpr(Left.Generate(env), Right.Generate(env), opr);
        }
    }

    [Serializable]
    public class RandomFloatValue : Value
    {
        [XmlAttribute]
        public float Max { get; set; }

        [XmlAttribute]
        public float Min { get; set; }

        [XmlAttribute]
        public float Step { get; set; }

        public RandomFloatValue()
        {
            Max = 1.0f;
            Min = 0.0f;
            Step = 1.0f;
        }

        public override float Get(Simulation.Actor actor)
        {
            var level = (int)Math.Ceiling((Max - Min) / Step) + 1;
            var ret = Min + Step * actor.World.Random.Next(level);
            if (ret > Max)
            {
                ret = Max;
            }
            return ret;
        }

        public override Expression Generate(GenerationEnvironment env)
        {
            var level = (int)Math.Ceiling((Max - Min) / Step) + 1;
            return new BiOpExpr(new ConstNumberExpr(Min),
                new BiOpExpr(
                    new BiOpExpr(ThisExpr.Instance.MakeIndex("rand").Call(), new ConstNumberExpr(level), BiOpExpr.Op.Mod),
                    new ConstNumberExpr(Step), BiOpExpr.Op.Multiply),
                BiOpExpr.Op.Add);
        }
    }
}
