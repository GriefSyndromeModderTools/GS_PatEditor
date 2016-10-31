using SharpDX.DirectSound;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.SoundEffect
{
    class SoundEngine
    {
        private DirectSound device;

        public SoundEngine(IntPtr hwnd)
        {
            DirectSound directSound = new DirectSound();
            directSound.SetCooperativeLevel(hwnd, CooperativeLevel.Priority);

            // Create PrimarySoundBuffer
            var desc = new SoundBufferDescription();
            desc.Flags = BufferFlags.PrimaryBuffer;
            desc.AlgorithmFor3D = Guid.Empty;

            var primaryBuffer = new PrimarySoundBuffer(directSound, desc);

            // Play the PrimarySound Buffer
            primaryBuffer.Play(0, PlayFlags.Looping);

            this.device = directSound;
        }

        public DirectSound Device
        {
            get
            {
                return device;
            }
        }

        private HashSet<Sound> _Sounds = new HashSet<Sound>();

        internal void OnNewSoundObject(Sound s)
        {
            _Sounds.Add(s);
        }

        internal void OnDisposeSoundObject(Sound s)
        {
            _Sounds.Remove(s);
        }

        internal void OnNewBufferCreated()
        {
            foreach (var s in _Sounds)
            {
                if (s.TryReduceBuffer())
                {
                    return;
                }
            }
        }

        public int BufferCount
        {
            get
            {
                return _Sounds.Sum(s => s.BufferCount);
            }
        }

        [STAThread]
        private static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            var path = @"E:\Games\[game]GRIEFSYNDROME\griefsyndrome\gs00\data\se\data\";
            var frm = new System.Windows.Forms.Form();
            var engine = new SoundEngine(frm.Handle);

            frm.Show();
            var sound = new Sound(engine, path + "1008.cv3");
            sound.Play();
            System.Windows.Forms.Application.Run(frm);
            System.Threading.Thread.Sleep(5000);
        }
    }
}
