﻿using GS_PatEditor.Editor.Editable;
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
    [Serializable]
    [LocalizedClassDisplayName(typeof(PlayerSkillAirBehavior))]
    public class PlayerSkillAirBehavior : Behavior
    {
        [XmlAttribute]
        [DefaultValue(true)]
        [LocalizedDescriptionAttribute("PlayerSkillAirBehavior_AutoCancel")]
        public bool AutoCancel { get; set; }

        [XmlElement]
        [LocalizedDescriptionAttribute("PlayerSkillAirBehavior_SitCancelSegment")]
        public int? SitCancelSegment { get; set; }

        [XmlElement]
        [LocalizedDescriptionAttribute("PlayerSkillAirBehavior_ResetHitSegments")]
        public SegmentSelector ResetHitSegments { get; set; }

        public PlayerSkillAirBehavior()
        {
            AutoCancel = true;
            ResetHitSegments = new SegmentSelector { Index = "*" };
        }

        public override void MakeEffects(ActionEffects effects)
        {
            effects.InitEffects.Add(new Effects.PlayerSkillInitEffect
            {
                AutoCancel = AutoCancel,
                IsInAir = true,
            });
            effects.UpdateEffects.Effects.Add(new IncreaseCountEffect());
            effects.SegmentFinishEffects.AddEffectToList(effects.SegmentCount - 1,
                new PlayerEndToFreeMoveEffect());
            if (SitCancelSegment.HasValue)
            {
                effects.SegmentStartEffects.AddEffectToList(SitCancelSegment.Value,
                    new SetLabelEffect
                    {
                        Label = Simulation.ActorLabelType.Sit,
                        Effect = PlayerBeginSitEffect.Instance,
                    });
            }
            SegmentSelectorHelper.MakeEffectsAsStart(effects, ResetHitSegments,
                Effects.ResetHitEffect.Instance);
        }
    }
}
