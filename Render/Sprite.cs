using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Render
{
    public class Sprite : IDisposable
    {
        private bool _Dirty;

        private Texture _Texture;
        public Texture Texture
        {
            get
            {
                return _Texture;
            }
            set
            {
                _Dirty = true;
                _Texture = value;
            }
        }

        #region Geometry Properties
        private float _Left;
        public float Left
        {
            get
            {
                return _Left;
            }
            set
            {
                _Dirty = true;
                _Left = value;
            }
        }

        private float _Top;
        public float Top
        {
            get
            {
                return _Top;
            }
            set
            {
                _Dirty = true;
                _Top = value;
            }
        }

        private float _OriginX;
        public float OriginX
        {
            get
            {
                return _OriginX;
            }
            set
            {
                _Dirty = true;
                _OriginX = value;
            }
        }

        private float _OriginY;
        public float OriginY
        {
            get
            {
                return _OriginY;
            }
            set
            {
                _Dirty = true;
                _OriginY = value;
            }
        }

        private float _RepeatX;
        public float RepeatX
        {
            get
            {
                return _RepeatX;
            }
            set
            {
                _Dirty = true;
                _RepeatX = value;
            }
        }

        private float _RepeatY;
        public float RepeatY
        {
            get
            {
                return _RepeatY;
            }
            set
            {
                _Dirty = true;
                _RepeatY = value;
            }
        }

        private bool _UseSize;

        private float _ScaleX;
        public float ScaleX
        {
            get
            {
                return _ScaleX;
            }
            set
            {
                _Dirty = true;
                _ScaleX = value;
                _UseSize = false;
            }
        }

        private float _ScaleY;
        public float ScaleY
        {
            get
            {
                return _ScaleY;
            }
            set
            {
                _Dirty = true;
                _ScaleY = value;
                _UseSize = false;
            }
        }

        private float _SizeX;
        public float SizeX
        {
            get
            {
                return _SizeX;
            }
            set
            {
                _Dirty = true;
                _SizeX = value;
                _UseSize = true;
            }
        }

        private float _SizeY;
        public float SizeY
        {
            get
            {
                return _SizeY;
            }
            set
            {
                _Dirty = true;
                _SizeY = value;
                _UseSize = true;
            }
        }

        private float _Rotation;
        public float Rotation
        {
            get
            {
                return _Rotation;
            }
            set
            {
                _Dirty = true;
                _Rotation = value;
            }
        }

        private float _Rotation0;
        public float Rotation0
        {
            get
            {
                return _Rotation0;
            }
            set
            {
                _Dirty = true;
                _Rotation0 = value;
            }
        }
        #endregion

        #region Color Properties

        private Vector4 Color = new Vector4(1, 1, 1, 1);

        public float Alpha
        {
            get
            {
                return Color.W;
            }
            set
            {
                Color.W = value;
            }
        }

        public float Red
        {
            get
            {
                return Color.X;
            }
            set
            {
                Color.X = value;
            }
        }

        public float Green
        {
            get
            {
                return Color.Y;
            }
            set
            {
                Color.Y = value;
            }
        }

        public float Blue
        {
            get
            {
                return Color.Z;
            }
            set
            {
                Color.Z = value;
            }
        }

        #endregion

        //only used in sprite setup functions
        public float RotationOffset { get; set; }

        private Device _Device;

        private readonly RenderEngine _RenderEngine;
        public RenderEngine RenderEngine { get { return _RenderEngine; } }

        private VertexBuffer _Buffer;
        private VertexDeclaration _Decl;
        private int _Stride;

        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            [VertexField(DeclarationUsage.Position)]
            public Vector4 pos;
            [VertexField(DeclarationUsage.TextureCoordinate)]
            public Vector4 tex;
            [VertexField(DeclarationUsage.Color)]
            public Vector4 col;
            //Don't add more fields
        }

        public Sprite(RenderEngine re)
        {
            _RenderEngine = re;
            _Device = re.Device;
            _ScaleX = 1.0f;
            _ScaleY = 1.0f;
            _RepeatX = 1.0f;
            _RepeatY = 1.0f;
        }

        public void Render()
        {
            if (_Buffer == null)
            {
                InitBuffer();
            }
            if (_Dirty)
            {
                FlushBuffer();
                _Dirty = false;
            }

            _Device.SetTexture(0, _Texture);
            _Device.SetStreamSource(0, _Buffer, 0, _Stride);
            _Device.VertexDeclaration = _Decl;
            _Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }

        private void InitBuffer()
        {
            _Stride = Utilities.SizeOf<Vertex>();
            _Decl = VertexReflection.CreateVertexDeclaration<Vertex>(_Device);
            _Buffer = new VertexBuffer(_Device, _Stride * 4, Usage.WriteOnly, VertexFormat.None, Pool.Managed);

            _Dirty = true;
        }

        private void FlushBuffer()
        {
            float t_l, t_t, t_r, t_b;

            float x = Left, y = Top;
            float sx = ScaleX, sy = ScaleY;

            //get image size
            {
                var surface = Texture.GetSurfaceLevel(0);
                var desc = surface.Description;
                t_r = desc.Width;
                t_b = desc.Height;
                t_l = 0;
                t_t = 0;

                if (_UseSize)
                {
                    sx = SizeX / t_r;
                    sy = SizeY / t_b;
                }
            }
            //apply origin
            {
                t_l -= OriginX;
                t_r -= OriginX;
                t_t -= OriginY;
                t_b -= OriginY;
            }
            //apply scale
            {
                t_l *= sx;
                t_r *= sx;
                t_t *= sy;
                t_b *= sy;
            }

            var stream = _Buffer.Lock(0, 0, LockFlags.Discard);

            //apply rotation

            stream.Write(new Vertex
            {
                pos = MakePosition(x, y, t_l, t_t),
                tex = new Vector4(0.0f, 0.0f, 0.0f, 0.0f),
                col = Color,
            });
            stream.Write(new Vertex
            {
                pos = MakePosition(x, y, t_r, t_t),
                tex = new Vector4(_RepeatX, 0.0f, 0.0f, 0.0f),
                col = Color,
            });
            stream.Write(new Vertex
            {
                pos = MakePosition(x, y, t_l, t_b),
                tex = new Vector4(0.0f, _RepeatY, 0.0f, 0.0f),
                col = Color,
            });
            stream.Write(new Vertex
            {
                pos = MakePosition(x, y, t_r, t_b),
                tex = new Vector4(_RepeatX, _RepeatY, 0.0f, 0.0f),
                col = Color,
            });
            stream.Dispose();

            _Buffer.Unlock();
        }

        private Vector4 MakePosition(float x, float y, float tx, float ty)
        {
            var r = _Rotation;
            var r0 = _Rotation0;
            var px = x + tx * (float)Math.Cos(r) - ty * (float)Math.Sin(r);
            var py = y + tx * (float)Math.Sin(r) + ty * (float)Math.Cos(r);
            return new Vector4(
                px * (float)Math.Cos(r0) - py * (float)Math.Sin(r0),
                px * (float)Math.Sin(r0) + py * (float)Math.Cos(r0),
                0.0f, 1.0f);
        }

        public void Dispose()
        {
            if (_Buffer != null)
            {
                _Buffer.Dispose();
                _Buffer = null;
            }
            if (_Decl != null)
            {
                _Decl.Dispose();
                _Decl = null;
            }
        }
    }
}
