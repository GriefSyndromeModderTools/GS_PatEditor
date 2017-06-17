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
    public enum ActorMemberType
    {
        x,
        y,
        vx,
        vy,
        rz,
        sx,
        sy,
        alpha,
        count,
    }

    [Serializable]
    public class ActorMemberValue : Value
    {
        [XmlAttribute]
        public ActorMemberType Type { get; set; }

        public override float Get(Simulation.Actor actor)
        {
            switch (Type)
            {
                case ActorMemberType.x:
                    return actor.X;
                case ActorMemberType.y:
                    return actor.Y;
                case ActorMemberType.vx:
                    return actor.VX;
                case ActorMemberType.vy:
                    return actor.VY;
                case ActorMemberType.rz:
                    return actor.Rotation;
                case ActorMemberType.sx:
                    return actor.ScaleX;
                case ActorMemberType.sy:
                    return actor.ScaleY;
                case ActorMemberType.alpha:
                    return actor.Alpha;
                case ActorMemberType.count:
                    return actor.ActionCount;
            }
            return 0;
        }

        public override Expression Generate(GenerationEnvironment env)
        {
            string index = null;
            switch (Type)
            {
                case ActorMemberType.x:
                    index = "x";
                    break;
                case ActorMemberType.y:
                    index = "y";
                    break;
                case ActorMemberType.vx:
                    index = "vx";
                    break;
                case ActorMemberType.vy:
                    index = "vy";
                    break;
                case ActorMemberType.rz:
                    index = "rz";
                    break;
                case ActorMemberType.sx:
                    index = "sx";
                    break;
                case ActorMemberType.sy:
                    index = "sy";
                    break;
                case ActorMemberType.alpha:
                    index = "alpha";
                    break;
                case ActorMemberType.count:
                    index = "count";
                    break;
            }
            if (index == null)
            {
                return new ConstNumberExpr(0);
            }
            var ret = ThisExpr.Instance.MakeIndex(index);
            if (Type == ActorMemberType.vx)
            {
                ret = new BiOpExpr(ret, ThisExpr.Instance.MakeIndex("direction"), BiOpExpr.Op.Multiply);
            }
            return ret;
        }
    }

    [Serializable]
    public class BulletParentMemberValue : Value
    {
        [XmlAttribute]
        public ActorMemberType Type { get; set; }

        public override float Get(Simulation.Actor actor)
        {
            actor = actor.Owner;
            switch (Type)
            {
                case ActorMemberType.x:
                    return actor.X;
                case ActorMemberType.y:
                    return actor.Y;
                case ActorMemberType.vx:
                    return actor.VX;
                case ActorMemberType.vy:
                    return actor.VY;
                case ActorMemberType.rz:
                    return actor.Rotation;
                case ActorMemberType.sx:
                    return actor.ScaleX;
                case ActorMemberType.sy:
                    return actor.ScaleY;
                case ActorMemberType.alpha:
                    return actor.Alpha;
                case ActorMemberType.count:
                    return actor.ActionCount;
            }
            return 0;
        }

        public override Expression Generate(GenerationEnvironment env)
        {
            string index = null;
            switch (Type)
            {
                case ActorMemberType.x:
                    index = "x";
                    break;
                case ActorMemberType.y:
                    index = "y";
                    break;
                case ActorMemberType.vx:
                    index = "vx";
                    break;
                case ActorMemberType.vy:
                    index = "vy";
                    break;
                case ActorMemberType.rz:
                    index = "rz";
                    break;
                case ActorMemberType.sx:
                    index = "sx";
                    break;
                case ActorMemberType.sy:
                    index = "sy";
                    break;
                case ActorMemberType.alpha:
                    index = "alpha";
                    break;
                case ActorMemberType.count:
                    index = "count";
                    break;
            }
            if (index == null)
            {
                return new ConstNumberExpr(0);
            }
            var ret = ActorVariableHelper.GenerateGet("SYS_parent").MakeIndex("wr").MakeIndex(index);
            if (Type == ActorMemberType.vx)
            {
                ret = new BiOpExpr(ret, ThisExpr.Instance.MakeIndex("direction"), BiOpExpr.Op.Multiply);
            }
            return ret;
        }
    }
}
