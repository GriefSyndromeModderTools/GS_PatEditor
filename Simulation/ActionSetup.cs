using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Simulation
{
    static class ActionSetup
    {
        private static void RunEffects(this IEnumerable<Pat.Effect> effectList, Simulation.Actor actor)
        {
            foreach (var e in effectList)
            {
                e.Run(actor);
            }
        }

        //set labels, run init effects (if specified)
        public static void SetupActorForAction(Actor actor, Pat.Action action, bool runInit)
        {
            var ae = new Pat.ActionEffects(actor.Actions, action);
            foreach (var b in action.Behaviors)
            {
                if (b.Enabled)
                {
                    b.MakeEffects(ae);
                }
            }
            ae.ProcessPostEffects();

            actor.UpdateLabel = ae.UpdateEffects.RunEffects;
            actor.StartKeyFrameLabel = ae.SegmentStartEffects.Select(list => (ActorLabel)list.RunEffects).ToArray();
            actor.EndKeyFrameLabel = ae.SegmentFinishEffects.Select(list => (ActorLabel)list.RunEffects).ToArray();
            if (runInit)
            {
                ae.InitEffects.RunEffects(actor);
            }
        }
    }
}
