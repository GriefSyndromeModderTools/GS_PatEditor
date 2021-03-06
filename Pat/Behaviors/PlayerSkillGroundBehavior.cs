﻿using GS_PatEditor.Editor.Editable;
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
    [LocalizedClassDisplayName(typeof(PlayerSkillGroundBehavior))]
    public class PlayerSkillGroundBehavior : Behavior
    {
        [XmlAttribute]
        [DefaultValue(true)]
        [LocalizedDescriptionAttribute("PlayerSkillGroundBehavior_AutoCancel")]
        public bool AutoCancel { get; set; }

        [XmlElement]
        [LocalizedDescriptionAttribute("PlayerSkillGroundBehavior_ResetHitSegments")]
        public SegmentSelector ResetHitSegments { get; set; }

        public PlayerSkillGroundBehavior()
        {
            AutoCancel = true;
            ResetHitSegments = new SegmentSelector { Index = "*" };
        }

        public override void MakeEffects(ActionEffects effects)
        {
            effects.InitEffects.Add(new Effects.PlayerSkillInitEffect
            {
                AutoCancel = AutoCancel,
                IsInAir = false,
            });
            effects.UpdateEffects.Effects.Add(new Effects.IncreaseCountEffect());
            effects.SegmentFinishEffects.AddEffectToList(effects.SegmentCount - 1,
                new Effects.PlayerEndToFreeMoveEffect());
            SegmentSelectorHelper.MakeEffectsAsStart(effects, ResetHitSegments,
                Effects.ResetHitEffect.Instance);
        }
    }
}
