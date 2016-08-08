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
    public class PlayerSkillInitPositionEffect : Effect, IHideFromEditor
    {
        [XmlAttribute]
        public bool IsInAir { get; set; }

        public override void Run(Simulation.Actor actor)
        {
            if (IsInAir)
            {
                actor.Y = -150.0f;
                actor.IsInAir = true;
            }
            else
            {
                actor.Y = 0.0f;
                actor.IsInAir = false;
            }
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return SimpleLineObject.Empty;
        }
    }

    [Serializable]
    public class PlayerSkillFallCancelEffect : SetLabelEffect, IHideFromEditor
    {
        public static readonly PlayerSkillFallCancelEffect Instance = new PlayerSkillFallCancelEffect();

        public PlayerSkillFallCancelEffect()
        {
            base.Label = Simulation.ActorLabelType.Fall;
            base.Effect = PlayerBeginFallEffect.Instance;
        }
    }

    [Serializable]
    public class PlayerSkillSitCancelEffect : SetLabelEffect, IHideFromEditor
    {
        public static readonly PlayerSkillSitCancelEffect Instance = new PlayerSkillSitCancelEffect();

        public PlayerSkillSitCancelEffect()
        {
            base.Label = Simulation.ActorLabelType.Sit;
            base.Effect = PlayerBeginSitEffect.Instance;
        }
    }

    [Serializable]
    public class PlayerAdjustDirectionEffect : Effect, IHideFromEditor
    {
        public static readonly PlayerAdjustDirectionEffect Instance = new PlayerAdjustDirectionEffect();

        public override void Run(Simulation.Actor actor)
        {
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return new ControlBlock(ControlBlockType.If, "this.input.x", new ILineObject[] {
                new SimpleLineObject("this.direction = this.input.x > 0 ? 1.0 : -1.0;"),
            }).Statement();
        }
    }


    [Serializable]
    public class PlayerSkillInitEffect : EffectListEffect
    {
        [XmlAttribute]
        public bool IsInAir
        {
            get
            {
                return _Position.IsInAir;
            }
            set
            {
                _Position.IsInAir = value;
            }
        }

        [XmlAttribute]
        public bool AutoCancel { get; set; }

        private PlayerSkillInitPositionEffect _Position = new PlayerSkillInitPositionEffect();
        private PlayerClearLabelEffect _ClearLabel = PlayerClearLabelEffect.Instance;
        private InitCountEffect _InitCount = InitCountEffect.Instance;
        private SetMotionEffect _SetMotion = new SetMotionEffect();
        private PlayerSkillFallCancelEffect _FallCancel = PlayerSkillFallCancelEffect.Instance;
        private PlayerSkillSitCancelEffect _SitCancel = PlayerSkillSitCancelEffect.Instance;
        private PlayerAdjustDirectionEffect _AdjustDirection = PlayerAdjustDirectionEffect.Instance;

        protected override IEnumerable<Effect> Effects
        {
            get
            {
                yield return _Position;
                yield return _ClearLabel;
                yield return _InitCount;
                yield return _SetMotion;
                if (AutoCancel)
                {
                    if (IsInAir)
                    {
                        yield return _SitCancel;
                    }
                    else
                    {
                        yield return _FallCancel;
                    }
                }
                yield return _AdjustDirection;
                yield break;
            }
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            var ret = new SimpleBlock(new ILineObject[] {
                _ClearLabel.Generate(env),
                _InitCount.Generate(env),
                _SetMotion.Generate(env),
                _AdjustDirection.Generate(env),
            }).Statement();
            if (AutoCancel)
            {
                if (IsInAir)
                {
                    ret = new SimpleBlock(new ILineObject[] {
                        ret,
                        _SitCancel.Generate(env),
                    }).Statement();
                }
                else
                {
                    ret = new SimpleBlock(new ILineObject[] {
                        ret,
                        _FallCancel.Generate(env),
                    }).Statement();
                }
            }
            return ret;
        }
    }

    [Serializable]
    public class ResetHitEffect : Effect
    {
        public static readonly ResetHitEffect Instance = new ResetHitEffect();

        public override void Run(Simulation.Actor actor)
        {
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return new SimpleLineObject("this.HitReset();");
        }
    }
}
