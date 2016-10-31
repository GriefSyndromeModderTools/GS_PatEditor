using GS_PatEditor.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Simulation
{
    class BulletActor : Actor
    {
        public BulletActor(World world, AnimationProvider animations, SystemAnimationProvider sysanimations,
            ActionProvider actions, ProjectSoundEffectCache se)
            : base(world, animations, sysanimations, actions, se)
        {
            ImmuneGravity = true;
            IsInAir = true;

            DefaultGravity = 0.0f;
            Priority = 300;
            CollisionEnabled = false;
        }

        public override void Update()
        {
            UpdateGravity();
            RunUpdateLabel();
            StepAnimation();
        }
    }
}
