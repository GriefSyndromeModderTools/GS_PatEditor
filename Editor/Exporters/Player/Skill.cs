using GS_PatEditor.Editor.Editable;
using GS_PatEditor.Pat.Effects;
using GS_PatEditor.Pat.Effects.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Editor.Exporters.Player
{
    public enum SkillKey
    {
        KeyA,
        KeyB,
        KeyC,
        KeyX,
        KeyY,
    }

    public static class SkillKeyExt
    {
        public static string GetKeyName(this SkillKey key)
        {
            switch (key)
            {
                case SkillKey.KeyA: return "b0";
                case SkillKey.KeyB: return "b1";
                case SkillKey.KeyC: return "b3";
                case SkillKey.KeyX: return "x";
                case SkillKey.KeyY: return "y";
                default: return "b0";
            }
        }
    }

    public enum DirectionHorizontal
    {
        Any,
        Empty,
        Front,
    }

    public enum DirectionVertical
    {
        Any,
        Empty,
        UpOnly,
        DownOnly,
    }

    public enum AirState
    {
        GroundOnly,
        AirOnly,
        Any,
    }

    [Serializable]
    [XmlInclude(typeof(NormalSkill))]
    public abstract class Skill
    {
        [XmlIgnore]
        [Browsable(false)]
        public abstract int Priority { get; }
    }

    [Serializable]
    public class NormalSkill : Skill, IEditableEnvironment, IDataNodeDisplayNameProvider
    {
        [XmlElement]
        [EditorChildNode("NormalSkill_Filter")]
        public Pat.Filter Filter;

        [XmlAttribute]
        public SkillKey Key { get; set; }

        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsRushSkill { get; set; }

        [XmlAttribute]
        public DirectionHorizontal X { get; set; }
        public bool ShouldSerializeX() { return X != DirectionHorizontal.Any; }

        [XmlAttribute]
        public DirectionVertical Y { get; set; }
        public bool ShouldSerializeY() { return Y != DirectionVertical.Any; }

        [XmlAttribute]
        public AirState AirState { get; set; }
        public bool ShouldSerializeAirState() { return AirState != Player.AirState.GroundOnly; }

        [XmlAttribute]
        public Pat.CancelLevel CancelLevel { get; set; }

        [XmlAttribute]
        public int MagicUse { get; set; }
        public bool ShouldSerializeMagicUse() { return MagicUse != 0; }

        [XmlAttribute]
        [TypeConverter(typeof(ActionIDConverter))]
        public string ActionID { get; set; }

        [XmlAttribute]
        [TypeConverter(typeof(ActionIDConverterNullable))]
        public string RushCancel { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public EditableEnvironment Environment { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public override int Priority
        {
            //get { return IsRushSkill ? 100 : 0; }
            get { return 0; }
        }

        [XmlIgnore]
        [Browsable(false)]
        public bool OverrideFullName
        {
            get { return true; }
        }

        [XmlIgnore]
        [Browsable(false)]
        public string DisplayName
        {
            get { return ActionID == null || ActionID.Length == 0 ? "NormalSkill" : ActionID; }
        }
    }
}
