using GS_PatEditor.Editor.Editable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat.Behaviors
{
    public enum PlayerSystemAnimationBehaviorType
    {
        Stand,
        Walk,
        Jump,
        JumpFront,
        Fall,
        Damage,
        Dead,
        Lost,
    }

    [Serializable]
    [LocalizedClassDisplayName(typeof(PlayerSystemAnimationBehavior))]
    public class PlayerSystemAnimationBehavior : Behavior
    {
        [XmlElement]
        [LocalizedDescriptionAttribute("PlayerSystemAnimationBehavior_PlayerSystemAnimationBehaviorType")]
        public PlayerSystemAnimationBehaviorType Type { get; set; }

        [XmlElement]
        [LocalizedDescriptionAttribute("PlayerSystemAnimationBehavior_IsStatic")]
        public bool IsStatic { get; set; }

        public override void MakeEffects(ActionEffects effects)
        {
            switch (Type)
            {
                case PlayerSystemAnimationBehaviorType.Stand:
                    effects.InitEffects.Add(new Effects.PlayerSkillInitEffect
                    {
                        AutoCancel = false,
                        IsInAir = false,
                    });
                    break;
                case PlayerSystemAnimationBehaviorType.Walk:
                    effects.InitEffects.Add(new Effects.PlayerSkillInitEffect
                    {
                        AutoCancel = false,
                        IsInAir = false,
                    });
                    if (!IsStatic)
                    {
                        effects.InitEffects.Add(new Effects.SetActorMemberEffect
                        {
                            Type = Effects.ActorMemberType.vx,
                            Value = new ConstValue { Value = 4.0f },
                        });
                    }
                    break;
                case PlayerSystemAnimationBehaviorType.Jump:
                    effects.InitEffects.Add(new Effects.PlayerSkillInitEffect
                    {
                        AutoCancel = true,
                        IsInAir = true,
                    });
                    effects.InitEffects.Add(new Effects.SetActorMemberEffect
                    {
                        Type = Effects.ActorMemberType.vy,
                        Value = new ConstValue { Value = -15.0f },
                    });
                    effects.InitEffects.Add(new Effects.SetActorMemberEffect
                    {
                        Type = Effects.ActorMemberType.y,
                        Value = new ConstValue { Value = 0.0f },
                    });
                    effects.SegmentFinishEffects.AddEffectToList(effects.SegmentCount - 1,
                        new Effects.PlayerEndToFreeMoveEffect());
                    break;
                case PlayerSystemAnimationBehaviorType.JumpFront:
                    effects.InitEffects.Add(new Effects.PlayerSkillInitEffect
                    {
                        AutoCancel = true,
                        IsInAir = true,
                    });
                    effects.InitEffects.Add(new Effects.SetActorMemberEffect
                    {
                        Type = Effects.ActorMemberType.vx,
                        Value = new ConstValue { Value = 4.0f },
                    });
                    effects.InitEffects.Add(new Effects.SetActorMemberEffect
                    {
                        Type = Effects.ActorMemberType.vy,
                        Value = new ConstValue { Value = -15.0f },
                    });
                    effects.InitEffects.Add(new Effects.SetActorMemberEffect
                    {
                        Type = Effects.ActorMemberType.y,
                        Value = new ConstValue { Value = 0.0f },
                    });
                    effects.SegmentFinishEffects.AddEffectToList(effects.SegmentCount - 1,
                        new Effects.PlayerEndToFreeMoveEffect());
                    break;
                case PlayerSystemAnimationBehaviorType.Fall:
                    effects.InitEffects.Add(new Effects.PlayerSkillInitEffect
                    {
                        AutoCancel = true,
                        IsInAir = true,
                    });
                    if (IsStatic)
                    {
                        effects.InitEffects.Add(new Effects.GravityEffect { Value = 0 });
                        effects.UpdateEffects.Add(new Effects.GravityEffect { Value = 0 });
                    }
                    effects.SegmentFinishEffects.AddEffectToList(effects.SegmentCount - 1,
                        new Effects.PlayerEndToFreeMoveEffect());
                    break;
                case PlayerSystemAnimationBehaviorType.Damage:
                    effects.InitEffects.Add(new Effects.PlayerSkillInitEffect
                    {
                        AutoCancel = false,
                        IsInAir = false,
                    });
                    effects.SegmentFinishEffects.AddEffectToList(effects.SegmentCount - 1,
                        new Effects.PlayerEndToFreeMoveEffect());
                    break;
            }
        }
    }
}
