﻿using GS_PatEditor.Pat.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat.Behaviors
{
    //this behavior is hide because it's not very easy to decide how much cycles to loop for
    [Serializable]
    public class LoopSegmentsBehavior : Behavior, IHideFromEditor
    {
        [XmlAttribute]
        public int Start { get; set; }

        [XmlAttribute]
        public int Number { get; set; }

        public override void MakeEffects(ActionEffects effects)
        {
            effects.SegmentFinishEffects.AddEffectToList(Start + Number,
                new SetSegmentEffect { Segment = Start });
        }
    }

    [Serializable]
    public class JumpSegmentBehavior : Behavior
    {
        [XmlElement]
        public int JumpFrom { get; set; }

        [XmlElement]
        public int? JumpTo { get; set; }

        public JumpSegmentBehavior()
        {
            JumpFrom = 0;
            JumpTo = 1;
        }

        public override void MakeEffects(ActionEffects effects)
        {
            if (JumpTo.HasValue)
            {
                effects.SegmentFinishEffects.AddEffectToList(JumpFrom, new SetSegmentEffect { Segment = JumpTo.Value });
            }
            else
            {
                effects.SegmentFinishEffects.AddEffectToList(JumpFrom, new PlayerEndToFreeMoveEffect());
            }
        }
    }
}
