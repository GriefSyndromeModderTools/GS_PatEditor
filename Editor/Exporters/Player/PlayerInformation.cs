using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Editor.Exporters.Player
{
    [Serializable]
    public class PlayerInformation
    {
        public PlayerInformation()
        {
            FallAttack = true;
            RegainCycle = 5;
            RegainRate = 2;
            AtkOffset = 0.1f;
            RushCount = 5;
        }

        [XmlElement]
        [DefaultValue(true)]
        public bool FallAttack { get; set; }

        [XmlElement]
        [DefaultValue(5)]
        public int RegainCycle { get; set; }

        [XmlElement]
        [DefaultValue(2)]
        public int RegainRate { get; set; }

        [XmlElement]
        public int MagicUse { get; set; }

        [XmlElement]
        [DefaultValue(0.1f)]
        public float AtkOffset { get; set; }

        [XmlElement]
        [DefaultValue(5)]
        public int RushCount { get; set; }
    }
}
