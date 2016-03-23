﻿using GS_PatEditor.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor.Panels
{
    class PreviewWindowPlaying : AbstractPreviewWindowContent
    {
        private readonly Simulation.World _World;
        private readonly Editor _Parent;

        private Sprite _Sprite;

        private class PatProjectAnimationProvider : Simulation.AnimationProvider
        {
            public Pat.Project Project;
            public string DefaultAnimation;

            public Pat.Animation GetAnimationByID(string id)
            {
                return Project.Actions
                    .Where(a => a.ActionID == (id == null ? DefaultAnimation : id) && a.Animation != null)
                    .Select(a => a.Animation)
                    .FirstOrDefault();
            }
        }

        private class PatProjectActionProvider : Simulation.ActionProvider
        {
            public Pat.Project Project;

            public Pat.Action GetActionByID(string id)
            {
                return Project.Actions.FirstOrDefault(a => a.ActionID == id);
            }
        }

        public PreviewWindowPlaying(Editor parent)
        {
            _Parent = parent;
            _World = new Simulation.World(_Parent.PreviewWindowUI.ControlWidth, _Parent.PreviewWindowUI.ControlHeight);

            _World.WhenFinished += PlayingFinished;
            _World.WhenError += PlayerError;

            var action = parent.CurrentAction;

            var actor = new Simulation.PlayerActor(_World,
                new PatProjectAnimationProvider { Project = parent.Project, DefaultAnimation = action.ActionID },
                new Simulation.SystemAnimationProvider(_Parent.Project),
                new PatProjectActionProvider { Project = parent.Project });

            Simulation.ActionSetup.SetupActorForAction(actor, action);

            _World.Add(actor);

            var sprites = parent.PreviewWindowUI.SpriteManager;
            _Sprite = sprites.GetSprite(0);

            _Parent.PreviewWindowUI.PreviewMoving.ResetScaleForPlay();
        }

        private void PlayingFinished()
        {
            _Parent.PreviewMode = FramePreviewMode.Pause;
        }

        private void PlayerError()
        {
            _Parent.PreviewMode = FramePreviewMode.Pause;
            MessageBox.Show("Error in playing.", "PatEditor",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public override void Render()
        {
            try
            {
                _World.Update();

                var window = _Parent.PreviewWindowUI;
                foreach (var actor in _World.OrderBy(a => a.Priority))
                {
                    var frame = actor.CurrentFrame;
                    if (frame != null && frame.ImageID != null)
                    {
                        var txt = _Parent.Project.ImageList.GetTexture(frame.ImageID, _Parent.PreviewWindowUI.Render);
                        if (txt != null)
                        {
                            _Sprite.SetupActor(txt, actor, window.SpriteMoving);
                            _Sprite.Render();
                        }
                    }
                }
            }
            catch
            {
                PlayerError();
            }
        }
    }
}
