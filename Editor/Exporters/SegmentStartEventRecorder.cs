using GS_PatEditor.Editor.Exporters.CodeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Editor.Exporters
{
    public class SegmentStartEventRecorder
    {
        private struct Entry
        {
            public int Motion;
            public int ABPostfix;
            public ILineObject Code;
        }
        private List<Entry> _Generated = new List<Entry>();

        public void AddAction(Pat.ActionEffects action, int motionId, GenerationEnvironment env, int abPostfix)
        {
            var code = GenerateList(action.SegmentStartEffects, env);
            _Generated.Add(new Entry { Motion = motionId, ABPostfix = abPostfix, Code = code });
        }

        private ILineObject GenerateList(List<Pat.EffectList> list, GenerationEnvironment env)
        {
            List<ILineObject> ret = new List<ILineObject>();
            for (int i = 0; i < list.Count; ++i)
            {
                var effects = list[i];
                ret.Add(new ControlBlock(ControlBlockType.If, "this.keyTake == " + i.ToString(),
                    effects.Effects.Select(e => e.Generate(env))).Statement());
            }
            return new SimpleBlock(ret).Statement();
        }

        public ILineObject Generate()
        {
            List<ILineObject> ret = new List<ILineObject>();
            foreach (var entry in _Generated)
            {
                var cond = "this.motion - this.u.CA == " + entry.Motion.ToString() +
                    " && this.u.variables.SYS_ABPostfix == " + entry.ABPostfix.ToString();
                ret.Add(new ControlBlock(ControlBlockType.If,
                    cond, new ILineObject[] { entry.Code }).Statement());
            }
            return new SimpleBlock(ret).Statement();
        }
    }
}
