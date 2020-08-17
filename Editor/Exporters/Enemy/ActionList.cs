using GS_PatEditor.Editor.Editable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Editor.Exporters.Enemy
{
    [Serializable]
    public class ActionList
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public int? Time { get; set; }

        [XmlElement(ElementName = "Item")]
        [EditorChildNode(null)]
        public ActionListItemList Items = new ActionListItemList();
    }
}
