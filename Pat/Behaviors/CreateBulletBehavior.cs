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

namespace GS_PatEditor.Pat.Behaviors
{
    [Serializable]
    [LocalizedClassDisplayName(typeof(CreateBulletBehavior))]
    public class CreateBulletBehavior : Behavior, IEditableEnvironment
    {
        [XmlElement]
        [EditorChildNode("CreateBulletBehavior_Time")]
        public Time Time;

        [XmlElement]
        [TypeConverter(typeof(ActionIDConverter))]
        [LocalizedDescriptionAttribute("CreateBulletBehavior_Bullet")]
        public string Bullet { get; set; }

        [XmlElement]
        [LocalizedDescriptionAttribute("CreateBulletBehavior_Direction")]
        public CreateBulletDirection Direction { get; set; }

        [XmlElement]
        [EditorChildNode("CreateBulletBehavior_Position")]
        public PointProvider Position;

        [XmlArray(ElementName = "AdditionalBehavior")]
        [EditorChildNode("CreateBulletBehavior_AdditionalBehaviors", false)]
        public BehaviorList AdditionalBehaviors = new BehaviorList();

        //trick to avoid infinite recursive
        private CreateBulletEffect _Effect = new CreateBulletEffect();

        public override void MakeEffects(ActionEffects effects)
        {
            _Effect.ActionName = Bullet;
            _Effect.Direction = Direction;
            _Effect.Position = Position;
            _Effect.AdditionalBehaviors = AdditionalBehaviors;
            Time.MakeEffects(effects, _Effect);
        }

        [XmlIgnore]
        [Browsable(false)]
        public EditableEnvironment Environment { get; set; }
    }
}
