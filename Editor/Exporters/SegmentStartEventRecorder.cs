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
        private Dictionary<int, ILineObject> _Generated = new Dictionary<int, ILineObject>();

        public void AddAction(Pat.ActionEffects action, int motionId, GenerationEnvironment env)
        {
            _Generated.Add(motionId, GenerateList(action.SegmentStartEffects, env));
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
                var motion = entry.Key;
                ret.Add(new ControlBlock(ControlBlockType.If,
                    "this.motion - this.u.CA == " + motion.ToString(), new ILineObject[] { entry.Value }).Statement());
            }
            return new SimpleBlock(ret).Statement();
        }
    }
}
