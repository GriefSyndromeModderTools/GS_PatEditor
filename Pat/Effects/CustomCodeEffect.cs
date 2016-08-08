using GS_PatEditor.Editor.Exporters.CodeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using EditorAttribute = System.ComponentModel.EditorAttribute;

namespace GS_PatEditor.Pat.Effects
{
    [Serializable]
    public class CustomCodeEffect : Effect
    {
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [XmlArray]
        public List<string> Code { get; set; }

        public CustomCodeEffect()
        {
            Code = new List<string>();
        }

        public override void Run(Simulation.Actor actor)
        {
        }

        public override Editor.Exporters.CodeFormat.ILineObject Generate(Editor.Exporters.GenerationEnvironment env)
        {
            return new SimpleBlock(Code.Select(line => new SimpleLineObject(line))).Statement();
        }
    }
}
