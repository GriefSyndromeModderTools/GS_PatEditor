using GS_PatEditor.Editor.Exporters;
using GS_PatEditor.Editor.Exporters.CodeFormat;
using GS_PatEditor.Pat.Effects.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat.Effects
{
    [Serializable]
    public class StartSkillEffect : Effect, IEditableEnvironment
    {
        [XmlElement]
        [TypeConverter(typeof(ActionIDConverter))]
        public string Action { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public EditableEnvironment Environment { get; set; }

        public override void Run(Simulation.Actor actor)
        {
            var action = actor.Actions.GetActionByID(Action);
            actor.SetMotion(action, 0);
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            var func = env.GetSkillFunctionForAction(Action);
            return new SimpleLineObject("this.u." + func + ".call(this);");
        }
    }
}
