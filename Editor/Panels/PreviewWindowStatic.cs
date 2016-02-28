﻿using GS_PatEditor.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Editor.Panels
{
    class PreviewWindowStatic : AbstractPreviewWindowContent
    {
        private readonly Editor _Parent;

        private Sprite _Sprite, _SpriteLineV, _SpriteLineH;
        private Sprite[] _SpriteListPhysical;

        public PreviewWindowStatic(Editor parent)
        {
            _Parent = parent;

            var r = parent.PreviewWindowUI.Render;
            var sprites = parent.PreviewWindowUI.SpriteManager;

            _Sprite = sprites.GetSprite(0);

            _SpriteLineH = sprites.GetSprite(1);
            _SpriteLineH.SetupLine(0x000000, 0, 0, 10000, 0);

            _SpriteLineV = sprites.GetSprite(2);
            _SpriteLineV.SetupLine(0x000000, 0, 0, 10000, 3.1415926f / 2);

            _SpriteListPhysical = sprites.GetRectangle(0);
        }

        public override void Render()
        {
            var frame = _Parent.EditorNode.Animation.Frame.FrameData;

            if (frame != null)
            {
                var txt = _Parent.Data.ImageList.GetTexture(frame.ImageID, _Parent.PreviewWindowUI.Render);
                var window = _Parent.PreviewWindowUI;
                _Sprite.SetupFrame(txt, frame, window);
                _Sprite.Render();
            }

            if (_Parent.EditorNode.Animation.Frame.AxisVisible)
            {
                _SpriteLineV.Render();
                _SpriteLineH.Render();
            }

            if (_Parent.EditorNode.Animation.Frame.PhysicalBoxVisible)
            {
                if (frame != null && frame.PhysicalBox != null)
                {
                    _SpriteListPhysical.SetupPhysical(0x00FF66, frame.PhysicalBox);
                    _SpriteListPhysical.Render();
                }
            }
        }
    }
}
