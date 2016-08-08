using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor.Panels.Tools.Move
{
    class SpriteMovingHandler : EditingPoint
    {
        private int SpriteMovingX, SpriteMovingY;

        public SpriteMovingHandler(Editor editor, Control ctrl)
        {
            var window = editor.PreviewWindowUI;

            var move = new MouseMovable(ctrl, MouseButtons.Left, 0, 0);
            move.FilterMouseDown += window.GetFilterForEditMode(FrameEditMode.Move);
            move.OnMovedDiff += delegate(int x, int y)
            {
                SpriteMovingX = (int)(-x / window.PreviewMoving.PreviewScale);
                SpriteMovingY = (int)(-y / window.PreviewMoving.PreviewScale);
            };
            move.OnMoveFinished += delegate()
            {
                var frame = editor.CurrentFrame;
                if (frame != null)
                {
                    frame.OriginX += SpriteMovingX;
                    frame.OriginY += SpriteMovingY;
                }
                SpriteMovingX = 0;
                SpriteMovingY = 0;
            };
            window.AccurateMove += delegate(int dx, int dy)
            {
                if (move.IsMoving || !move.Enabled)
                {
                    return;
                }
                var frame = editor.CurrentFrame;
                if (frame != null)
                {
                    frame.OriginX -= dx;
                    frame.OriginY -= dy;
                }
            };
        }

        public int OffsetX
        {
            get { return SpriteMovingX; }
        }

        public int OffsetY
        {
            get { return SpriteMovingY; }
        }
    }
}
