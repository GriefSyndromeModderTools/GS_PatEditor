using SharpDX;
using SharpDX.DirectSound;
using SharpDX.Multimedia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.SoundEffect
{
    class Sound
    {
        private SoundEngine _Engine;
        private int _Channels, _Rate, _Bits;

        private byte[] _RawBuffer;
        private List<SecondarySoundBuffer> _Buffers = new List<SecondarySoundBuffer>();

        private void _Init(SoundEngine engine, int channels, int rate, int bits, int size, Stream data)
        {
            _Engine = engine;
            _Channels = channels;
            _Rate = rate;
            _Bits = bits;

            _RawBuffer = new byte[size];
            data.Read(_RawBuffer, 0, size);

            _Buffers.Add(CreateBuffer());
        }

        public Sound(SoundEngine engine, int channels, int rate, int bits, int size, Stream data)
        {
            _Init(engine, channels, rate, bits, size, data);
        }

        public Sound(SoundEngine engine, string filename)
        {
            using (var file = File.OpenRead(filename))
            {
                using (var br = new BinaryReader(file))
                {
                    var audioFormat = br.ReadInt16();
                    if (audioFormat != 1)
                    {
                        throw new IOException("invalid cv3 file");
                    }
                    var channels = br.ReadInt16();
                    var rate = br.ReadInt32();
                    var bytesPerSecond = br.ReadInt32();
                    var align = br.ReadInt16();
                    if (align != 2)
                    {
                        throw new IOException("invalid cv3 file");
                    }
                    var bits = br.ReadInt16();
                    if (bytesPerSecond != rate * bits / 8 * channels)
                    {
                        throw new IOException("invalid cv3 file");
                    }
                    var zero = br.ReadInt16();
                    if (zero != 0)
                    {
                        throw new IOException("invalid cv3 file");
                    }
                    var len = br.ReadInt32();

                    _Init(engine, channels, rate, bits, len, file);
                }
            }
        }

        public void Dispose()
        {
            DisposeBuffer();
        }

        private SecondarySoundBuffer CreateBuffer()
        {
            var secondaryBufferDesc = new SoundBufferDescription();
            secondaryBufferDesc.AlgorithmFor3D = Guid.Empty;
            secondaryBufferDesc.BufferBytes = _RawBuffer.Length;
            secondaryBufferDesc.Format = new WaveFormat(_Rate, _Bits, _Channels);
            secondaryBufferDesc.Flags =
                BufferFlags.GetCurrentPosition2 |
                BufferFlags.ControlPositionNotify |
                BufferFlags.GlobalFocus |
                BufferFlags.ControlVolume |
                BufferFlags.StickyFocus;

            var buffer = new SecondarySoundBuffer(_Engine.Device, secondaryBufferDesc);

            // Get Capabilties from secondary sound buffer
            var capabilities = buffer.Capabilities;

            // Lock the buffer
            DataStream dataPart2;
            var dataPart1 = buffer.Lock(0, capabilities.BufferBytes, LockFlags.EntireBuffer, out dataPart2);
            int len = capabilities.BufferBytes;

            dataPart1.Write(_RawBuffer, 0, len);

            buffer.Unlock(dataPart1, dataPart2);

            return buffer;
        }

        internal bool TryReduceBuffer()
        {
            for (int i = 0; i < _Buffers.Count; ++i)
            {
                var s = _Buffers[i].Status & (int)BufferStatus.Playing;
                if (s == 0)
                {
                    _Buffers[i].Dispose();
                    _Buffers.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void DisposeBuffer()
        {
            var buffers = new List<SecondarySoundBuffer>(_Buffers);
            _Buffers.Clear();
            foreach (var b in buffers)
            {
                b.Dispose();
            }
        }

        public void Play()
        {
            var free = _Buffers.FirstOrDefault(b => (b.Status & (int)BufferStatus.Playing) == 0);
            if (free == null)
            {
                free = CreateBuffer();
                _Buffers.Add(free);
            }
            free.Play(0, PlayFlags.None);
        }

        public TimeSpan Length
        {
            get
            {
                return new TimeSpan((long)(TimeSpan.TicksPerSecond *
                    1.0 * _RawBuffer.Length / (_Rate * _Bits / 8 * _Channels)));
            }
        }

        public int BufferCount
        {
            get
            {
                return _Buffers.Count;
            }
        }
    }
}
