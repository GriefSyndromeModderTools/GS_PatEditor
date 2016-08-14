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

        //[XmlArray]
        //public EffectList InitEffects = new EffectList();

        //[XmlArray]
        //public EffectList UpdateEffects = new EffectList();

        //[XmlArray]
        //public List<EffectList> SegmentFinishEffects = new List<EffectList>();

        //[XmlArray]
        //public List<EffectList> SegmentStartEffects = new List<EffectList>();

        [XmlArray]
        public List<Behavior> Behaviors = new List<Behavior>();

        //[XmlIgnore]
        //public bool ContainsLowLevelEffects
        //{
        //    get
        //    {
        //        return InitEffects.Count > 0 ||
        //            UpdateEffects.Count > 0 ||
        //            SegmentStartEffects.Count > 0 ||
        //            SegmentFinishEffects.Count > 0;
        //    }
        //}
    }
}
