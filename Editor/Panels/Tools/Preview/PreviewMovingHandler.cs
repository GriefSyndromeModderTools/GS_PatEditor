﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor.Panels.Tools.Preview
{
    class PreviewMovingHandler
    {
        private readonly PreviewWindow _Parent;
        private readonly Control _Control;

        private int _PreviewX, _PreviewY;
        private int _PreviewMovingX, _PreviewMovingY;
        private float _PreviewScale = 1.0f;

        public event Action SceneMoved;

        public float PreviewScale
        {
            get
            {
                return _PreviewScale;
            }
        }

        private MouseMovable MovablePreview;

        public PreviewMovingHandler(PreviewWindow parent, Control ctrl)
        {
            _Parent = parent;
            _Control = ctrl;

            _PreviewX = ctrl.Width / 2;
            _PreviewY = ctrl.Height / 2;
            parent.Render.Transform.X = _PreviewX;
            parent.Render.Transform.Y = _PreviewY;

            MovablePreview = new MouseMovable(ctrl, MouseButtons.Middle, _PreviewMovingX, _PreviewMovingY);
            MovablePreview.OnMovedDiff += delegate(int x, int y)
            {
                _PreviewMovingX = x;
                _PreviewMovingY = y;

                parent.Render.Transform.X = _PreviewX + _PreviewMovingX;
                parent.Render.Transform.Y = _PreviewY + _PreviewMovingY;

                OnSceneMoved();
            };
            MovablePreview.OnMoveFinished += delegate()
            {
                _PreviewX += _PreviewMovingX;
                _PreviewY += _PreviewMovingY;
                _PreviewMovingX = 0;
                _PreviewMovingY = 0;

                parent.Render.Transform.X = _PreviewX;
                parent.Render.Transform.Y = _PreviewY;

                OnSceneMoved();
            };

            //mouse wheel zoom in/out
            ctrl.FindForm().MouseWheel += _Control_MouseWheel;
            ctrl.MouseWheel += _Control_MouseWheel;
        }

        private void OnSceneMoved()
        {
            if (SceneMoved != null)
            {
                SceneMoved();
            }
        }

        private void _Control_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!_Control.IsMouseInRange())
            {
                return;
            }
            if (e.Delta < 0)
            {
                SetScale(_PreviewScale * 0.9f);
            }
            else if (e.Delta > 0)
            {
                SetScale(_PreviewScale / 0.9f);
            }
            ((HandledMouseEventArgs)e).Handled = true;
        }

        private void SetScale(float newScale)
        {
            if (newScale < 0.1f || newScale > 10.0f)
            {
                return;
            }

            //client position of mouse
            var pClient = _Control.PointToClient(Control.MousePosition);

            //position on scaled scene
            float pSSx = (pClient.X - _PreviewX) / _PreviewScale;
            float pSSy = (pClient.Y - _PreviewY) / _PreviewScale;

            //this point is still on pClient
            float newX = pClient.X - pSSx * newScale;
            float newY = pClient.Y - pSSy * newScale;

            _PreviewX = (int)newX;
            _PreviewY = (int)newY;
            _PreviewScale = newScale;

            MovablePreview.SetPosition(_PreviewX, _PreviewY);

            _Parent.Render.Transform.Scale = _PreviewScale;
            _Parent.Render.Transform.X = _PreviewX;
            _Parent.Render.Transform.Y = _PreviewY;

            OnSceneMoved();
        }

        public void ResetScale(float scale)
        {
            _PreviewX = _Control.Width / 2;
            _PreviewY = _Control.Height / 2;
            _Parent.Render.Transform.X = _PreviewX;
            _Parent.Render.Transform.Y = _PreviewY;

            _PreviewScale = scale;
            _Parent.Render.Transform.Scale = _PreviewScale;
        }

        public void ResetScaleForPlay()
        {
            int offsetY = 0;
            {
                var parentCtrl = _Control.Parent as ScrollableControl;
                if (parentCtrl != null)
                {
                    offsetY = parentCtrl.ClientSize.Height / 2;
                }
            }
            _PreviewX = _Control.Width / 2;
            _PreviewY = _Control.Height / 2 + offsetY;
            _Parent.Render.Transform.X = _PreviewX;
            _Parent.Render.Transform.Y = _PreviewY;

            _PreviewScale = 1.0f;
            _Parent.Render.Transform.Scale = _PreviewScale;
        }

        //TODO transform in point

        public float TransformXSpriteToClient(float x)
        {
            return (_PreviewX + _PreviewMovingX) + _PreviewScale * x;
        }

        public float TransformYSpriteToClient(float y)
        {
            return (_PreviewY + _PreviewMovingY) + _PreviewScale * y;
        }

        public float TransformXClientToSprite(float x)
        {
            return (x - _PreviewX - _PreviewMovingX) / _PreviewScale;
        }

        public float TransformYClientToSprite(float y)
        {
            return (y - _PreviewY - _PreviewMovingY) / _PreviewScale;
        }
    }
}
