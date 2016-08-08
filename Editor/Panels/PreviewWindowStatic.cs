using GS_PatEditor.Editor.Panels.Tools;
using GS_PatEditor.Render;
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
        private Sprite[] _SpriteListBorder;
        private Sprite[] _SpritePoints;

        public PreviewWindowStatic(Editor parent)
        {
            _Parent = parent;

            var r = parent.PreviewWindowUI.Render;
            var sprites = parent.PreviewWindowUI.SpriteManager;

            _Sprite = sprites.GetSprite(0);

            _SpriteLineH = sprites.GetSprite(1);
            _SpriteLineH.SetupDashLine(0x000000, 10000, 10);
            _SpriteLineH.SetRotationOffset(0);
            _SpriteLineH.SetupPosition(0, 0, 0);

            _SpriteLineV = sprites.GetSprite(2);
            _SpriteLineV.SetupDashLine(0x000000, 10000, 10);
            _SpriteLineV.SetRotationOffset(1);
            _SpriteLineV.SetupPosition(0, 0, 0);

            _SpriteListPhysical = sprites.GetRectangle(0);
            _SpriteListBorder = sprites.GetRectangle(1);

            _SpritePoints = new[] {
                sprites.GetSprite(3),
                sprites.GetSprite(4),
                sprites.GetSprite(5),
                sprites.GetSprite(6),
                sprites.GetSprite(7),
                sprites.GetSprite(8),
            };

            uint[] colors = new uint[] { 0xDC2020, 0xFFFF00, 0x20F010 };
            for (int i = 0; i < 3; ++i)
            {
                _SpritePoints[i * 2].SetupLine(colors[i], 8);
                _SpritePoints[i * 2].SetRotationOffset(0.5f);
                _SpritePoints[i * 2 + 1].SetupLine(colors[i], 8);
                _SpritePoints[i * 2 + 1].SetRotationOffset(1.5f);
            }
        }

        private class BorderRectProvider : EditingPhysicalBox
        {
            public float Left { get; set; }
            public float Top { get; set; }
            public float Width { get; set; }
            public float Height { get; set; }
        }

        private BorderRectProvider _BorderProvider = new BorderRectProvider();

        public override void Render()
        {
            var frame = _Parent.CurrentFrame;
            var window = _Parent.PreviewWindowUI;

            if (frame != null && frame.ImageID != null)
            {
                var txt = _Parent.Project.ImageList.GetTexture(frame.ImageID, _Parent.PreviewWindowUI.Render);
                if (txt != null)
                {
                    _Sprite.SetupFrame(txt, frame, window.SpriteMoving);
                    _Sprite.Render();
                    if (_Parent.BorderVisible)
                    {
                        _SpriteListBorder.SetupBorder(frame, txt, window.SpriteMoving);
                        _SpriteListBorder.Render();
                    }
                }
            }

            if (true)
            {
                var spriteManagerRef = _Parent.PreviewWindowUI.SpriteManagerRef;
                var opacity = _Parent.PreviewWindowUI.ReferenceOpacity;
                for (int i = 0; i < _Parent.PreviewWindowUI.ReferenceList.Count; ++i)
                {
                    var info = _Parent.PreviewWindowUI.ReferenceList[i];
                    if (info.Visible)
                    {
                        var s = spriteManagerRef.GetSprite(i);
                        var txt = _Parent.Project.ImageList.GetTexture(info.Frame.ImageID,
                            _Parent.PreviewWindowUI.Render);
                        if (txt != null)
                        {
                            s.SetupFrame(txt, info.Frame, EmptyEditingPoint.Instance);
                            s.Alpha *= opacity / 100.0f;
                            s.Render();
                        }
                    }
                }
            }

            if (_Parent.AxisVisible)
            {
                _SpriteLineV.Render();
                _SpriteLineH.Render();
            }

            if (_Parent.PhysicalBoxVisible)
            {
                if (frame != null && frame.PhysicalBox != null)
                {
                    _SpriteListPhysical.SetupPhysical(0x00FF66, window.PhysicalEditing.PhysicalBoxData);
                    _SpriteListPhysical.Render();
                }
            }

            var sprites = _Parent.PreviewWindowUI.SpriteManager;
            int rectIndex = 2;
            if (_Parent.HitBoxVisible)
            {
                foreach (var box in _Parent.PreviewWindowUI.HitEditing.BoxData.Data)
                {
                    var s = sprites.GetRectangle(rectIndex++);
                    s.SetupHit(0x00A2E8, box);
                    s.Render();
                }
            }
            if (_Parent.AttackBoxVisible)
            {
                foreach (var box in _Parent.PreviewWindowUI.AttackEditing.BoxData.Data)
                {
                    var s = sprites.GetRectangle(rectIndex++);
                    s.SetupHit(0xE8A200, box);
                    s.Render();
                }
            }
            if (_Parent.PointVisible)
            {
                int index = 0;
                Sprite s;
                foreach (var point in _Parent.PreviewWindowUI.PointEditing.Points())
                {
                    //sprite 1
                    s = _SpritePoints[index++];
                    s.SetupPosition(point.X, point.Y, 0);
                    s.Render();
                    //sprite 2
                    s = _SpritePoints[index++];
                    s.SetupPosition(point.X, point.Y, 0);
                    s.Render();
                }
            }
        }
    }
}
