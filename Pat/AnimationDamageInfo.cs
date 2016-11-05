using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat
{
    public enum AttackType
    {
        None,
        Light,
        Heavy,
        Smash,
    }

    [Serializable]
    public struct HitStop
    {
        [XmlElement]
        public int Self;
        [XmlElement]
        public int Opponent;
    }

    [Serializable]
    public struct HitKnockback
    {
        [XmlElement]
        public int SpeedX;
        [XmlElement]
        public int SpeedY;
        [XmlElement]
        public int Gravity;
    }

    [Serializable]
    public class AnimationDamageInfo
    {
        [XmlElement]
        public AttackType AttackType;

        [XmlElement]
        public int BaseDamage;

        [XmlElement]
        public HitStop HitStop;

        [XmlElement]
        public HitKnockback Knockback;

        [XmlElement]
        public int SoundEffect;
    }
}
