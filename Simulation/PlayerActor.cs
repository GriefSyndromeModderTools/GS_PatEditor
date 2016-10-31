using GS_PatEditor.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Simulation
{
    class PlayerActor : Actor
    {
        public PlayerActor(World world, AnimationProvider animations, SystemAnimationProvider sysanimations,
            ActionProvider actions, ProjectSoundEffectCache se)
            : base(world, animations, sysanimations, actions, se)
        {
            ImmuneGravity = false;
            DefaultGravity = 1.0f;
            Priority = 100;

            CollisionEnabled = true;
        }

        public override void Update()
        {
            UpdateGravity();
            RunUpdateLabel();
            StepAnimation();
        }
    }
}
