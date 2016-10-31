using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat
{
    [Serializable]
    public class Action
    {
        [XmlAttribute]
        public string ActionID;

        [XmlAttribute]
        public string Category;

        [XmlAttribute]
        public string ImageID;

        [XmlArray]
        public List<AnimationSegment> Segments = new List<AnimationSegment>();

        [XmlArray]
        public List<Behavior> Behaviors = new List<Behavior>();
    }
}
