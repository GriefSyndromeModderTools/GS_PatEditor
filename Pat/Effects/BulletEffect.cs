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
    public class BulletInitEffect : Effect
    {
        private SetMotionEffect _SetMotion = new SetMotionEffect();

        public override void Run(Simulation.Actor actor)
        {
            _SetMotion.Run(actor);
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return new SimpleBlock(new ILineObject[] {
                new SimpleLineObject("this.ShotInit(t);"),
                new ControlBlock(ControlBlockType.If, "t.owner in this.actor", new ILineObject[] {
                    new SimpleLineObject("this.u.uu <- this.actor[t.owner].u.uu;"),
                    new SimpleLineObject("this.u.CA <- this.actor[t.owner].u.CA;"),
                }).Statement(),
                new ControlBlock(ControlBlockType.Else, new ILineObject[] {
                    new SimpleLineObject("this.u.uu <- { uuu = null };"),
                    new SimpleLineObject("this.u.CA <- 0;"),
                }).Statement(),
                //get parent actor from table t (before setmotion, where SYS_parent may be used)
                ActorVariableHelper.GenerateSet("SYS_parent", new IdentifierExpr("t").MakeIndex("flag1")),
                _SetMotion.Generate(env),
            }).Statement();
        }
    }

    [Serializable]
    public class TimeStopCheckEffect : Effect
    {
        public override void Run(Simulation.Actor actor)
        {
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return new ControlBlock(ControlBlockType.If, "this.UpdateStopCheck()", new ILineObject[] {
                new SimpleLineObject("return false;"),
            }).Statement();
        }
    }

    [Serializable]
    public class BulletSetSpeedEffect : Effect
    {
        [XmlElement]
        [EditorChildNode("BulletSetSpeedEffect_Speed")]
        public Value Speed;

        [XmlElement]
        [EditorChildNode("BulletSetSpeedEffect_Rotation")]
        public Value Rotation;

        public override void Run(Simulation.Actor actor)
        {
            var s = Speed.Get(actor);
            var r = Rotation.Get(actor);

            actor.Rotation = r;
            actor.VX = (float)Math.Cos(r) * s;
            actor.VY = (float)Math.Sin(r) * s;
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            var r = new IdentifierExpr("r");
            var s = new IdentifierExpr("s");
            var vx = new BiOpExpr(ThisExpr.Instance.MakeIndex("cos").Call(r), s, BiOpExpr.Op.Multiply);
            vx = new BiOpExpr(vx, ThisExpr.Instance.MakeIndex("direction"), BiOpExpr.Op.Multiply);
            var vy = new BiOpExpr(ThisExpr.Instance.MakeIndex("sin").Call(r), s, BiOpExpr.Op.Multiply);

            return new SimpleBlock(new ILineObject[] {
                new LocalVarStatement("r", Rotation.Generate(env)),
                new LocalVarStatement("s", Speed.Generate(env)),
                ThisExpr.Instance.MakeIndex("rz").Assign(r).Statement(),
                ThisExpr.Instance.MakeIndex("vx").Assign(vx).Statement(),
                ThisExpr.Instance.MakeIndex("vy").Assign(vy).Statement(),
                new SimpleLineObject("this.SetCollisionRotation(0.0, 0.0, this.rz);"),
            }).Statement();
        }
    }

    [Serializable]
    public class BulletUpdateCollisionEffect : Effect
    {
        public override void Run(Simulation.Actor actor)
        {
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return new SimpleBlock(new ILineObject[] {
                new SimpleLineObject("this.SetCollisionScaling(this.sx, this.sy, 1.0);"),
                new SimpleLineObject("this.SetCollisionRotation(0.0, 0.0, this.rz);"),
            }).Statement();
        }
    }

    [Serializable]
    public class BulletIncreaseSpeedEffect : Effect
    {
        [XmlAttribute]
        public float Value { get; set; }

        public override void Run(Simulation.Actor actor)
        {
            actor.VX += Value * (float)Math.Cos(actor.Rotation);
            actor.VY += Value * (float)Math.Sin(actor.Rotation);
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            var vx = ThisExpr.Instance.MakeIndex("vx");
            var vy = ThisExpr.Instance.MakeIndex("vy");
            var val = new ConstNumberExpr(Value);
            var rz = ThisExpr.Instance.MakeIndex("rz");
            var vxd = new BiOpExpr(val, ThisExpr.Instance.MakeIndex("cos").Call(rz), BiOpExpr.Op.Multiply);
            var vyd = new BiOpExpr(val, ThisExpr.Instance.MakeIndex("sin").Call(rz), BiOpExpr.Op.Multiply);
            vxd = new BiOpExpr(vxd, ThisExpr.Instance.MakeIndex("direction"), BiOpExpr.Op.Multiply);

            return new SimpleBlock(new ILineObject[] {
                vx.Assign(new BiOpExpr(vx, vxd, BiOpExpr.Op.Add)).Statement(),
                vy.Assign(new BiOpExpr(vy, vyd, BiOpExpr.Op.Add)).Statement(),
            }).Statement();
        }
    }

    [Serializable]
    public class BulletRotationFromSpeedEffect : Effect
    {
        public override void Run(Simulation.Actor actor)
        {
            actor.Rotation = (float)Math.Atan2(actor.VY, actor.VX);
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return new SimpleLineObject("this.rz = this.atan2(this.vy, this.vx * this.direction);");
        }
    }

    [Serializable]
    public class BulletReduceAlphaEffect : Effect
    {
        [XmlAttribute]
        public float Value { get; set; }

        [XmlAttribute]
        public bool Release { get; set; }

        [XmlAttribute]
        public int Segment { get; set; }

        [XmlElement]
        public int? SegmentAfterFinish { get; set; }

        public override void Run(Simulation.Actor actor)
        {
            if (actor.CurrentSegmentIndex != Segment)
            {
                return;
            }

            if (actor.Alpha < Value)
            {
                BulletEffectHelper.SimulateEnd(actor, Release, SegmentAfterFinish);
            }
            else
            {
                actor.Alpha -= Value;
            }
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            var val = new ConstNumberExpr(Value);
            var alpha = ThisExpr.Instance.MakeIndex("alpha");

            return new ControlBlock(ControlBlockType.If, "this.keyTake == " + Segment, new ILineObject[] {
                new ControlBlock(ControlBlockType.If, new BiOpExpr(alpha, val, BiOpExpr.Op.Greater), new ILineObject[] {
                    alpha.Assign(new BiOpExpr(alpha, val, BiOpExpr.Op.Minus)).Statement(),
                }).Statement(),
                new ControlBlock(ControlBlockType.Else, new ILineObject[] {
                    BulletEffectHelper.GenerateEnd(Release, SegmentAfterFinish),
                }).Statement(),
            }).Statement();
        }
    }

    [Serializable]
    public class BulletFollowingOwnerInitEffect : Effect
    {
        [XmlAttribute]
        public string CheckInstance { get; set; }

        public override void Run(Simulation.Actor actor)
        {
            if (CheckInstance != null && CheckInstance.Length != 0)
            {
                actor.Owner.Variables[CheckInstance] = new Simulation.ActorVariable
                {
                    Type = Simulation.ActorVariableType.Actor,
                    Value = actor,
                };
            }
            actor.Variables["SYS_follow_relx"] = new Simulation.ActorVariable
            {
                Type = Simulation.ActorVariableType.Float,
                Value = actor.X - actor.Owner.X,
            };
            actor.Variables["SYS_follow_rely"] = new Simulation.ActorVariable
            {
                Type = Simulation.ActorVariableType.Float,
                Value = actor.X - actor.Owner.Y,
            };
            actor.Variables["SYS_follow_dir_p"] = new Simulation.ActorVariable
            {
                Type = Simulation.ActorVariableType.Float,
                Value = actor.Owner.InversedDirection ? -1.0f : 1.0f,
            };
            actor.Variables["SYS_follow_dir_s"] = new Simulation.ActorVariable
            {
                Type = Simulation.ActorVariableType.Float,
                Value = actor.InversedDirection ? -1.0f : 1.0f,
            };
            actor.Variables["SYS_follow_motion"] = new Simulation.ActorVariable
            {
                Type = Simulation.ActorVariableType.Unknown,
                Value = actor.Owner.CurrentAction.ActionID,
            };
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            var owner = ActorVariableHelper.GenerateGet("SYS_parent");

            List<ILineObject> ret = new List<ILineObject>();
            if (CheckInstance != null && CheckInstance.Length != 0)
            {
                ret.Add(ActorVariableHelper.GenerateSet(owner.MakeIndex("wr"),
                    CheckInstance, ThisExpr.Instance.MakeIndex("name")));
            }

            //save relative position
            ret.Add(ActorVariableHelper.GenerateSet("SYS_follow_relx",
                new BiOpExpr(ThisExpr.Instance.MakeIndex("x"),
                    owner.MakeIndex("wr").MakeIndex("x"),
                    BiOpExpr.Op.Minus)));
            ret.Add(ActorVariableHelper.GenerateSet("SYS_follow_rely",
                new BiOpExpr(ThisExpr.Instance.MakeIndex("y"),
                    owner.MakeIndex("wr").MakeIndex("y"),
                    BiOpExpr.Op.Minus)));
            ret.Add(ActorVariableHelper.GenerateSet("SYS_follow_dir_p",
                owner.MakeIndex("wr").MakeIndex("direction")));
            ret.Add(ActorVariableHelper.GenerateSet("SYS_follow_dir_s",
                ThisExpr.Instance.MakeIndex("direction")));
            ret.Add(ActorVariableHelper.GenerateSet("SYS_follow_motion",
                owner.MakeIndex("wr").MakeIndex("motion")));
            return new SimpleBlock(ret).Statement();
        }
    }

    [Serializable]
    public class BulletFollowingOwnerUpdateEffect : Effect
    {
        [XmlAttribute]
        public string CheckInstance { get; set; }

        [XmlAttribute]
        public bool IgnoreRotation { get; set; }

        [XmlElement]
        public bool ReleaseIfCheckFailed { get; set; }

        [XmlElement]
        public int? SegmentCheckFailed { get; set; }

        [XmlAttribute]
        public bool FailIfParentMotionChanged { get; set; }

        public override void Run(Simulation.Actor actor)
        {
            var owner = actor.Owner;
            if (owner != null && !owner.IsReleased)
            {
                if (CheckInstance != null && CheckInstance.Length != 0)
                {
                    if (!owner.Variables.ContainsKey(CheckInstance) ||
                        owner.Variables[CheckInstance].Value as Simulation.Actor != actor)
                    {
                        BulletEffectHelper.SimulateEnd(actor, ReleaseIfCheckFailed, SegmentCheckFailed);
                        return;
                    }
                }
                if (FailIfParentMotionChanged &&
                    owner.CurrentAction.ActionID != (string)actor.Variables["SYS_follow_motion"].Value)
                {
                    BulletEffectHelper.SimulateEnd(actor, ReleaseIfCheckFailed, SegmentCheckFailed);
                    return;
                }

                actor.X = owner.X +
                    (actor.Owner.InversedDirection ? -1.0f : 1.0f) * (float)actor.Variables["SYS_follow_dir_p"].Value * 
                    (float)actor.Variables["SYS_follow_relx"].Value;
                actor.Y = owner.Y + (float)actor.Variables["SYS_follow_relx"].Value;

                actor.InversedDirection = actor.Owner.InversedDirection;
                if ((float)actor.Variables["SYS_follow_dir_p"].Value != 
                    (float)actor.Variables["SYS_follow_dir_s"].Value)
                {
                    actor.InversedDirection = !actor.InversedDirection;
                }

                if (!IgnoreRotation)
                {
                    actor.Rotation = owner.Rotation;
                }
            }
            else
            {
                actor.Release();
            }
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            var ownerActor = new IdentifierExpr("ownerActor");
            
            List<ILineObject> ret = new List<ILineObject>();
            ret.Add(new LocalVarStatement("ownerActor", ActorVariableHelper.GenerateGet("SYS_parent").MakeIndex("wr")));

            BiOpExpr cond = null;
            if (CheckInstance != null && CheckInstance.Length != 0)
            {
                cond = new BiOpExpr(ActorVariableHelper.GenerateGet(ownerActor, CheckInstance),
                            ThisExpr.Instance.MakeIndex("name"), BiOpExpr.Op.NotEqual);
            }
            if (FailIfParentMotionChanged)
            {
                var cond2 = new BiOpExpr(ownerActor.MakeIndex("motion"),
                        ActorVariableHelper.GenerateGet("SYS_follow_motion"), BiOpExpr.Op.NotEqual);
                if (cond != null)
                {
                    cond = new BiOpExpr(cond, cond2, BiOpExpr.Op.Or);
                }
                else
                {
                    cond = cond2;
                }
            }
            if (cond != null)
            {
                ret.Add(new ControlBlock(ControlBlockType.If, cond,
                    new ILineObject[] {
                        BulletEffectHelper.GenerateEnd(ReleaseIfCheckFailed, SegmentCheckFailed),
                    }).Statement());
            }

            var dirChanged = new BiOpExpr(ownerActor.MakeIndex("direction"),
                ActorVariableHelper.GenerateGet("SYS_follow_dir_p"), BiOpExpr.Op.Multiply);
            var x = new BiOpExpr(ownerActor.MakeIndex("x"),
                new BiOpExpr(ActorVariableHelper.GenerateGet("SYS_follow_relx"),
                    dirChanged, BiOpExpr.Op.Multiply),
                BiOpExpr.Op.Add);
            var y = new BiOpExpr(ownerActor.MakeIndex("y"),
                ActorVariableHelper.GenerateGet("SYS_follow_rely"), BiOpExpr.Op.Add);
            var dir = new BiOpExpr(dirChanged,
                ActorVariableHelper.GenerateGet("SYS_follow_dir_s"), BiOpExpr.Op.Multiply);

            var setRotation = ThisExpr.Instance.MakeIndex("rz").Assign(ownerActor.MakeIndex("rz")).Statement();
            ret.AddRange(new ILineObject[] {
                new ControlBlock(ControlBlockType.If, "ownerActor != null", new ILineObject[] {
                    ThisExpr.Instance.MakeIndex("x").Assign(x).Statement(),
                    ThisExpr.Instance.MakeIndex("y").Assign(y).Statement(),
                    ThisExpr.Instance.MakeIndex("direction").Assign(dir).Statement(),
                    IgnoreRotation ? new SimpleLineObject("") : setRotation,
                }).Statement(),
                new ControlBlock(ControlBlockType.Else, new ILineObject[] {
                        ThisExpr.Instance.MakeIndex("Release").Call().Statement(),
                }).Statement(),
            });

            return new SimpleBlock(ret).Statement();
        }
    }

    class BulletEffectHelper
    {
        public static void SimulateEnd(Simulation.Actor a, bool r, int? s)
        {
            if (r)
            {
                a.Release();
            }
            else if (s.HasValue)
            {
                a.SetMotion(a.CurrentAction, s.Value);
            }
        }

        public static ILineObject GenerateEnd(bool r, int? s)
        {
            if (r)
            {
                return ThisExpr.Instance.MakeIndex("Release").Call().Statement();
            }
            else if (s.HasValue)
            {
                return new SimpleLineObject("this.SetMotion(this.motion, " + s.Value + ");");
            }
            else
            {
                return new SimpleLineObject("");
            }
        }
    }
}
