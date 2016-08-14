using GS_PatEditor.Editor.Editable;
using GS_PatEditor.Editor.Exporters;
using GS_PatEditor.Editor.Exporters.CodeFormat;
using GS_PatEditor.Pat.Effects.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat.Effects
{
    public static class SetMotionEffectHelper
    {
        public static ILineObject Generate(GenerationEnvironment env, ILineObject effect)
        {
            var func = env.GetSegmentStartEventHandlerFunctionName();
            if (func == null || func.Length == 0)
            {
                return effect;
            }
            return new SimpleBlock(new ILineObject[] {
                effect,
                new SimpleLineObject("this.u.uu.uuu." + func + ".call(this);"),
            }).Statement();
        }
    }

    [Serializable]
    public class AnimationContinueEffect : Effect
    {
        public override void Run(Simulation.Actor actor)
        {
            actor.SetMotion(actor.CurrentAction, actor.CurrentSegmentIndex + 1);
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            var ret = ThisExpr.Instance.MakeIndex("SetMotion").Call(
                ThisExpr.Instance.MakeIndex("motion"),
                new BiOpExpr(ThisExpr.Instance.MakeIndex("keyTake"), new ConstNumberExpr(1), BiOpExpr.Op.Add)
                ).Statement();
            return SetMotionEffectHelper.Generate(env, ret);
        }
    }

    [TypeConverter(typeof(EnumDisplayNameEnumConverterEnumConverter))]
    public enum CreateBulletDirection
    {
        [LocalizedEnumDisplayName(typeof(CreateBulletDirection), "Same")]
        Same,
        [LocalizedEnumDisplayName(typeof(CreateBulletDirection), "Opposite")]
        Opposite,
    }

    [Serializable]
    public class CreateBulletEffect : Effect, IEditableEnvironment
    {
        [XmlElement]
        [TypeConverter(typeof(ActionIDConverter))]
        public string ActionName { get; set; }

        [XmlElement]
        [EditorChildNode("CreateBulletEffect_Position")]
        public PointProvider Position;

        [XmlElement]
        public CreateBulletDirection Direction { get; set; }

        public override void Run(Simulation.Actor actor)
        {
            var bullet = new Simulation.BulletActor(actor.World,
                actor.Animations.SetDefault(ActionName), null, actor.Actions);
            var point = Position.GetPointForActor(actor);

            bullet.Owner = actor;
            bullet.X = point.X;
            bullet.Y = point.Y;
            bullet.InversedDirection = Direction == CreateBulletDirection.Same ?
                actor.InversedDirection : !actor.InversedDirection;

            var action = actor.Actions.GetActionByID(ActionName);
            if (action != null)
            {
                Simulation.ActionSetup.SetupActorForAction(bullet, action, true);
                actor.World.Add(bullet);
            }
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            var funcName = env.GenerateActionAsActorInit(ActionName);
            var dir = ThisExpr.Instance.MakeIndex("direction");

            if (Direction == CreateBulletDirection.Opposite)
            {
                dir = new UnOpExpr(dir, UnOpExpr.Op.Sub);
            }
            return new SimpleBlock(new ILineObject[] {
                //create t
                new SimpleLineObject("local t = this.DefaultShotTable();"),

                //set parent actor
                new SimpleLineObject("t.flag1 = { wr = this.weakref() };"),

                //TODO setup t

                //create actor
                ThisExpr.Instance.MakeIndex("world2d").MakeIndex("CreateActor").Call(
                    Position.GenerateX(ThisExpr.Instance, env),
                    Position.GenerateY(ThisExpr.Instance, env),
                    dir,
                    ThisExpr.Instance.MakeIndex("u").MakeIndex("uu").MakeIndex("uuu").MakeIndex(funcName),
                    new IdentifierExpr("t")
                ).Statement(),

                //clear actor reference (safety)
                new SimpleLineObject("t.flag1 = null;"),
            }).Statement();
        }

        [XmlIgnore]
        [Browsable(false)]
        public EditableEnvironment Environment { get; set; }
    }

    [Serializable]
    public class ReleaseActorEffect : Effect
    {
        public override void Run(Simulation.Actor actor)
        {
            actor.Release();
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return ThisExpr.Instance.MakeIndex("Release").Call().Statement();
        }
    }

    [Serializable]
    public class SetMotionEffect : Effect, IEditableEnvironment, IHideFromEditor
    {
        [XmlAttribute]
        [TypeConverter(typeof(ActionIDConverter))]
        public string Animation { get; set; }

        [XmlAttribute]
        public int Segment { get; set; }

        public override void Run(Simulation.Actor actor)
        {
            actor.SetMotion(Animation == "" ? null : Animation, Segment);
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            var id = env.GetActionID(Animation);
            var ret = ThisExpr.Instance.MakeIndex("SetMotion").Call(
                new BiOpExpr(new ConstNumberExpr(id),
                    ThisExpr.Instance.MakeIndex("u").MakeIndex("CA"),
                    BiOpExpr.Op.Add),
                new ConstNumberExpr(Segment)).Statement();
            return SetMotionEffectHelper.Generate(env, ret);
        }

        [XmlIgnore]
        [Browsable(false)]
        public EditableEnvironment Environment { get; set; }
    }

    [Serializable]
    public class SetSegmentEffect : Effect
    {
        [XmlAttribute]
        public int Segment { get; set; }

        public override void Run(Simulation.Actor actor)
        {
            actor.SetMotion(actor.CurrentAction, Segment);
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            var ret = new SimpleLineObject("this.SetMotion(this.motion, " + Segment + ");");
            return SetMotionEffectHelper.Generate(env, ret);
        }
    }

    [Serializable]
    public class SetMotionRandomSegmentEffect : Effect, IEditableEnvironment
    {
        [XmlAttribute]
        [TypeConverter(typeof(ActionIDConverter))]
        public string Animation { get; set; }

        [XmlAttribute]
        public int SegmentCount { get; set; }

        public override void Run(Simulation.Actor actor)
        {
            actor.SetMotion(Animation == "" ? null : Animation, actor.World.Random.Next(SegmentCount));
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            var id = env.GetActionID(Animation);
            var segment = new BiOpExpr(ThisExpr.Instance.MakeIndex("rand").Call(),
                new ConstNumberExpr(SegmentCount), BiOpExpr.Op.Mod);
            var ret = ThisExpr.Instance.MakeIndex("SetMotion").Call(new ConstNumberExpr(id), segment).Statement();
            return SetMotionEffectHelper.Generate(env, ret);
        }

        [XmlIgnore]
        [Browsable(false)]
        public EditableEnvironment Environment { get; set; }
    }


    [Serializable]
    public class InitCountEffect : Effect
    {
        public static readonly InitCountEffect Instance = new InitCountEffect();

        public override void Run(Simulation.Actor actor)
        {
            actor.ActionCount = 0;
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return new SimpleLineObject("this.count = 0;");
        }
    }

    [Serializable]
    public class IncreaseCountEffect : Effect
    {
        public override void Run(Simulation.Actor actor)
        {
            actor.ActionCount += 1;
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return new SimpleLineObject("this.count++;");
        }
    }

    [Serializable]
    public class SetLabelEffect : Effect
    {
        [XmlAttribute]
        public Simulation.ActorLabelType Label { get; set; }

        [XmlElement]
        [EditorChildNode("SetLabelEffect_Effect")]
        public Effect Effect;

        public override void Run(Simulation.Actor actor)
        {
            switch (Label)
            {
                case Simulation.ActorLabelType.Fall:
                    actor.FallLabel = Effect.Run;
                    break;
                case Simulation.ActorLabelType.Sit:
                    actor.SitLabel = Effect.Run;
                    break;
                case Simulation.ActorLabelType.Hit:
                    actor.HitEvent = Effect.Run;
                    break;
            }
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            var func = new FunctionBlock("", new string[0], new ILineObject[] { Effect.Generate(env) });
            switch (Label)
            {
                case Simulation.ActorLabelType.Fall:
                    return ThisExpr.Instance.MakeIndex("fallLabel").Assign(func.AsExpression()).Statement();
                case Simulation.ActorLabelType.Sit:
                    return ThisExpr.Instance.MakeIndex("sitLabel").Assign(func.AsExpression()).Statement();
                case Simulation.ActorLabelType.Hit:
                    return ThisExpr.Instance.MakeIndex("hitEvent").Assign(func.AsExpression()).Statement();
                default:
                    throw new Exception();
            }
        }
    }

    [Serializable]
    public class SetActorMemberEffect : Effect
    {
        [XmlAttribute]
        public ActorMemberType Type { get; set; }

        [XmlElement]
        [EditorChildNode("SetActorMemberEffect_Value")]
        public Value Value;

        public override void Run(Simulation.Actor actor)
        {
            var val = Value.Get(actor);
            switch (Type)
            {
                case ActorMemberType.x:
                    actor.X = val;
                    break;
                case ActorMemberType.y:
                    actor.Y = val;
                    break;
                case ActorMemberType.vx:
                    actor.VX = val;
                    break;
                case ActorMemberType.vy:
                    actor.VY = val;
                    break;
                case ActorMemberType.rz:
                    actor.Rotation = val;
                    break;
                case ActorMemberType.sx:
                    actor.ScaleX = val;
                    break;
                case ActorMemberType.sy:
                    actor.ScaleY = val;
                    break;
                case ActorMemberType.alpha:
                    actor.Alpha = val;
                    break;
                case ActorMemberType.count:
                    actor.ActionCount = (int)val;
                    break;
            }
        }

        public override ILineObject Generate(GenerationEnvironment env)
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
                return new SimpleLineObject("");
            }
            var val = Value.Generate(env);
            if (Type == ActorMemberType.vx)
            {
                val = new BiOpExpr(val, ThisExpr.Instance.MakeIndex("direction"), BiOpExpr.Op.Multiply);
            }
            return new BiOpExpr(ThisExpr.Instance.MakeIndex(index), val, BiOpExpr.Op.Assign).Statement();
        }
    }

    [Serializable]
    public class SetMaskEffect : Effect
    {
        [XmlAttribute]
        public int CollisionMask { get; set; }

        [XmlAttribute]
        public int CallbackMask { get; set; }

        public override void Run(Simulation.Actor actor)
        {
            //TODO support mask in simulation
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            if (CollisionMask != -1 && CallbackMask == -1)
            {
                return new SimpleLineObject("");
            }
            else if (CallbackMask == -1)
            {
                return new SimpleLineObject("this.collisionMask = " + CollisionMask + ";");
            }
            else if (CollisionMask == -1)
            {
                return new SimpleLineObject("this.callbackMask = " + CallbackMask + ";");
            }
            else
            {
                return new SimpleBlock(new ILineObject[] {
                    new SimpleLineObject("this.collisionMask = " + CollisionMask + ";"),
                    new SimpleLineObject("this.callbackMask = " + CallbackMask + ";"),
                }).Statement();
            }
        }
    }

    [Serializable]
    public class SetHitEffectEffect : Effect, IEditableEnvironment
    {
        [XmlAttribute]
        [TypeConverter(typeof(ActionIDConverter))]
        public string Action { get; set; }

        public override void Run(Simulation.Actor actor)
        {
            //not supported
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            var funcName = env.GenerateActionAsActorInit(Action);
            return ThisExpr.Instance.MakeIndex("hitEffect").Assign(
                ThisExpr.Instance.MakeIndex("u").MakeIndex("uu").MakeIndex("uuu").MakeIndex(funcName)).Statement();
        }

        [XmlIgnore]
        [Browsable(false)]
        public EditableEnvironment Environment { get; set; }
    }

    [Serializable]
    public class SetPriorityEffect : Effect
    {
        [XmlElement]
        public int Value { get; set; }

        public override void Run(Simulation.Actor actor)
        {
            //not supported
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return ThisExpr.Instance.MakeIndex("priority").Assign(new ConstNumberExpr(Value)).Statement();
        }
    }
}
