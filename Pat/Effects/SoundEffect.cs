using GS_PatEditor.Editor.Exporters;
using GS_PatEditor.Editor.Exporters.CodeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat.Effects
{
    [Serializable]
    public class SoundEffect : Effect
    {
        [XmlAttribute]
        public int Index { get; set; }

        public override void Run(Simulation.Actor actor)
        {
            actor.SoundEffects.PlaySound(Index);
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return ThisExpr.Instance.MakeIndex("PlaySE").Call(new ConstNumberExpr(Index)).Statement();
        }
    }
}
