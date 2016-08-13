using GS_PatEditor.Editor.Editable;
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
    public class ActorFloatVariableValue : Value
    {
        [XmlAttribute]
        public string Name { get; set; }

        public override float Get(Simulation.Actor actor)
        {
            if (!actor.Variables.ContainsKey(Name))
            {

            }
            var val = actor.Variables[Name];
            if (val.Type == Simulation.ActorVariableType.Float)
            {
                return (float)val.Value;
            }
            return 0;
        }

        public override Expression Generate(GenerationEnvironment env)
        {
            return ActorVariableHelper.GenerateGet(Name);
        }
    }

    [Serializable]
    public class ActorSetFloatVariableEffect : Effect
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement]
        [EditorChildNode("Value")]
        public Value Value;

        public override void Run(Simulation.Actor actor)
        {
            actor.Variables[Name] = new Simulation.ActorVariable()
            {
                Type = Simulation.ActorVariableType.Float,
                Value = Value.Get(actor),
            };
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return ActorVariableHelper.GenerateSet(Name, Value.Generate(env));
        }
    }
}
