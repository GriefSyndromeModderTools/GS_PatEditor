using GS_PatEditor.Pat;
using GS_PatEditor.SoundEffect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Editor
{
    public class ProjectSoundEffectCache
    {
        private Editor _Editor;
        private readonly SoundEngine _Engine;
        private Dictionary<int, Sound> _CachedSound = new Dictionary<int, Sound>();

        internal ProjectSoundEffectCache(Editor editor, IntPtr hwnd)
        {
            _Editor = editor;
            _Engine = new SoundEngine(hwnd);
            editor.ProjectReset += delegate()
            {
                _CachedSound.Clear();
            };
        }

        public void PlaySound(int index)
        {
            Sound s;
            if (!_CachedSound.TryGetValue(index, out s))
            {
                var res = _Editor.Project.FindResource(ProjectDirectoryUsage.SoundEffect, index.ToString("d4") + ".cv3");
                if (res == null)
                {
                    return;
                }
                s = new Sound(_Engine, res);
                _CachedSound[index] = s;
            }
            s.Play();
        }
    }
}
