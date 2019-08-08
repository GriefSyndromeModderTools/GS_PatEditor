using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat
{
    public enum CancelLevel
    {
        //for invalid value/not available
        None,
        //zero value in original pat file
        Free,
        Light,
        Long,
        Heavy,
        Magic,
        Highest,

        //jump is only for player skill
        Jump,
    }
    [Serializable]
    public class AnimationSegment
    {
        //Note on serialization of this class:
        //XML will serialize cancellable with the property
        //clipboard will serialize cancellable with field

        [XmlElement(ElementName = "Frame")]
        public List<Frame> Frames;

        [XmlElement(IsNullable = false)]
        public AnimationDamageInfo Damage;

        #region JumpCancellable
        [XmlIgnore]
        public AnimationCancellableInfo JumpCancellable;
        [XmlAttribute(AttributeName = "JumpCancellable")]
        public string JumpCancellableString
        {
            get
            {
                return JumpCancellable == null ? null : JumpCancellable.StartFrom.ToString();
            }
            set
            {
                if (value == null)
                {
                    JumpCancellable = null;
                }
                else
                {
                    if (JumpCancellable == null)
                    {
                        JumpCancellable = new AnimationCancellableInfo();
                    }
                    JumpCancellable.StartFrom = Int32.Parse(value);
                }
            }
        }
        #endregion

        #region SkillCancellable
        [XmlIgnore]
        public AnimationCancellableInfo SkillCancellable;
        [XmlAttribute(AttributeName = "SkillCancellable")]
        public string SkillCancellableString
        {
            get
            {
                return SkillCancellable == null ? null : SkillCancellable.StartFrom.ToString();
            }
            set
            {
                if (value == null)
                {
                    SkillCancellable = null;
                }
                else
                {
                    if (SkillCancellable == null)
                    {
                        SkillCancellable = new AnimationCancellableInfo();
                    }
                    SkillCancellable.StartFrom = Int32.Parse(value);
                }
            }
        }
        #endregion

        [XmlAttribute]
        public CancelLevel CancelLevel;
        public bool ShouldSerializeCancelLevel()
        {
            return CancelLevel != Pat.CancelLevel.None;
        }

        [XmlAttribute]
        public bool IsLoop;
    }
}
