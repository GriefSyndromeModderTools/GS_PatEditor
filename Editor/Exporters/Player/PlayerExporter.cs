using GS_PatEditor.Pat.Effects;
using GS_PatEditor.Pat.Effects.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Editor.Exporters.Player
{
    [Serializable]
    public class PlayerExporterAnimations : IEditableEnvironment
    {
        [XmlElement]
        [TypeConverter(typeof(ActionIDConverter))]
        public string Stand { get; set; }

        [XmlElement]
        [TypeConverter(typeof(ActionIDConverter))]
        public string Walk { get; set; }

        [XmlElement]
        [TypeConverter(typeof(ActionIDConverter))]
        public string JumpUp { get; set; }

        [XmlElement]
        [TypeConverter(typeof(ActionIDConverter))]
        public string JumpFront { get; set; }

        [XmlElement]
        [TypeConverter(typeof(ActionIDConverter))]
        public string Fall { get; set; }

        [XmlElement]
        [TypeConverter(typeof(ActionIDConverter))]
        public string FallAttack { get; set; }

        [XmlElement]
        [TypeConverter(typeof(ActionIDConverter))]
        public string Damage { get; set; }

        [XmlElement]
        [TypeConverter(typeof(ActionIDConverter))]
        public string Dead { get; set; }

        [XmlElement]
        [TypeConverter(typeof(ActionIDConverter))]
        public string Lost { get; set; }

        [XmlElement]
        [Editor(typeof(ImageIDEditor), typeof(UITypeEditor))]
        public string ReviveLight { get; set; }

        [XmlElement]
        [Editor(typeof(ImageIDEditor), typeof(UITypeEditor))]
        public string ReviveDark { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public EditableEnvironment Environment { get; set; }
    }

    [Serializable]
    public class PlayerExporter : AbstractExporter, IEditableEnvironment
    {
        [XmlElement]
        public int BaseIndex { get; set; }

        [XmlElement]
        public string PlayerName { get; set; }

        [XmlElement]
        public string ScriptFileName { get; set; }

        [XmlElement]
        public string PlayerStringType { get; set; }

        [XmlElement]
        public int PlayerIntType { get; set; }

        [XmlElement]
        public int PlayerFontType { get; set; }

        [XmlElement]
        public PlayerExporterAnimations Animations = new PlayerExporterAnimations();

        [XmlElement]
        public PlayerInformation PlayerInformation = new PlayerInformation();

        [XmlArray]
        public List<Skill> Skills = new List<Skill>();

        private Pat.Project _Project;
        private Dictionary<string, int> _GeneratedActionID = new Dictionary<string, int>();
        private int _NextFreeActionID;
        private SegmentStartEventRecorder _SSERecorder;

        [XmlIgnore]
        [Browsable(false)]
        public SegmentStartEventRecorder SSERecorder
        {
            get
            {
                return _SSERecorder;
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        public GenerationEnvironment GenEnv { get; private set; }

        [XmlIgnore]
        [Browsable(false)]
        public Pat.Project Project { get { return _Project; } }

        public override void ShowOptionDialog(Pat.Project proj)
        {
            Environment = new EditableEnvironment(proj);
            Animations.Environment = Environment;

            var dialog = new PlayerExporterOptionsForm(proj, this);
            dialog.ShowDialog();
        }

        public override void Export(Pat.Project proj)
        {
            _Project = proj;
            _GeneratedActionID.Clear();
            _SSERecorder = new SegmentStartEventRecorder();

            _NextFreeActionID = 20;

            //init script
            var actorCommon = base.AddCodeFile("actorCommon.add.cv4");
            PlayerInitFunctionGenerator.Generate(this, proj, actorCommon);

            //actions
            ExportAction(Animations.Stand, 0);
            ExportAction(Animations.Walk, 1);
            ExportAction(Animations.JumpUp, 3);
            ExportAction(Animations.JumpFront, 4);
            ExportAction(Animations.Fall, 8);
            ExportAction(Animations.FallAttack, 9);
            ExportAction(Animations.Damage, 10);
            ExportReviveLightAction(17);
            ExportAction(Animations.Dead, 18);
            ExportAction(Animations.Lost, 19);

            var playerScript = base.AddCodeFile(ScriptFileName + ".cv4");

            SystemActionFunctionGenerator.Generate(this, proj, playerScript);

            this.GenEnv = SkillGenerator.CreateEnv(this, playerScript);
            SkillGenerator.GenerateSkills(this, playerScript);
            SkillGenerator.GenerateAliasAssignment(this);
            this.GenEnv = null; //TODO handle exception

            SkillGenerator.GenerateStartMotionFunction(_SSERecorder, playerScript);

            ExportAction(Animations.Stand, 100);
            ExportAction(Animations.Stand, 101);
            ExportAction(Animations.Stand, 102);
            ExportAction(Animations.Stand, 103);
        }

        private void ExportAction(string name, int id)
        {
            if (name == null || name.Length == 0)
            {
                return;
            }

            var action = _Project.Actions.FirstOrDefault(a => a.ActionID == name);
            if (action != null)
            {
                base.AddNormalAnimation(action, id + BaseIndex);
                if (this.GenEnv != null)
                {
                    //_SSERecorder.AddAction(action, id + BaseIndex, this.GenEnv);
                }
            }
        }

        private void ExportReviveLightAction(int id)
        {
            var light = Animations.ReviveLight;
            var dark = Animations.ReviveDark;
            if (light == null || dark == null)
            {
                return;
            }

            var action = new Pat.Action
            {
                Segments = new List<Pat.AnimationSegment>()
                {
                    //segment 0
                    new Pat.AnimationSegment
                    {
                        CancelLevel = Pat.CancelLevel.Highest,
                        IsLoop = true,
                        Frames = new List<Pat.Frame>()
                        {
                            CreateFrame(light, 255, 100, 64, 113, 3, true),
                            CreateFrame(light, 255, 75, 64, 129, 3, true),
                        },
                    },
                    //segment 1
                    new Pat.AnimationSegment
                    {
                        CancelLevel = Pat.CancelLevel.Highest,
                        IsLoop = true,
                        Frames = new List<Pat.Frame>()
                        {
                            CreateFrame(dark, 255, 100, 64, 64, 3, false),
                            CreateFrame(dark, 192, 100, 64, 64, 3, false),
                        },
                    },
                    //segment 2
                    new Pat.AnimationSegment
                    {
                        CancelLevel = Pat.CancelLevel.Highest,
                        IsLoop = true,
                        Frames = new List<Pat.Frame>()
                        {
                            CreateFrame(light, 255, 200, 64, 89, 3, false),
                            CreateFrame(light, 255, 190, 64, 91, 3, false),
                        },
                    },
                    //segment 3
                    new Pat.AnimationSegment
                    {
                        CancelLevel = Pat.CancelLevel.Highest,
                        IsLoop = true,
                        Frames = new List<Pat.Frame>()
                        {
                            CreateFrame(light, 255, 25, 64, 64, 4, false),
                            CreateFrame(light, 255, 22, 64, 64, 4, false),
                        },
                    },
                },
            };
            base.AddNormalAnimation(action, id + BaseIndex);
        }

        private Pat.Frame CreateFrame(string img, int alpha, int scale, int ox, int oy, int dur, bool phy)
        {
            return new Pat.Frame
            {
                ImageID = img,
                Alpha = alpha / 255.0f,
                ScaleX = scale,
                ScaleY = scale,
                OriginX = ox,
                OriginY = oy,
                Duration = dur,
                Points = new List<Pat.FramePoint>()
                {
                    new Pat.FramePoint(),
                    new Pat.FramePoint(),
                    new Pat.FramePoint(),
                },
                PhysicalBox = phy ?
                    new Pat.PhysicalBox { X = -20, Y = -90, W = 40, H = 90 } :
                    null,
                AttackBoxes = new List<Pat.Box>(),
                HitBoxes = new List<Pat.Box>(),
            };
        }

        public int GetActionID(string name)
        {
            int ret;
            if (_GeneratedActionID.TryGetValue(name, out ret))
            {
                return ret;
            }
            ret = _NextFreeActionID++;
            ExportAction(name, ret);
            _GeneratedActionID.Add(name, ret);
            return ret;
        }

        public Pat.Action GetAction(string name)
        {
            return _Project.Actions.FirstOrDefault(a => a.ActionID == name);
        }

        [XmlIgnore]
        [Browsable(false)]
        public EditableEnvironment Environment { get; set; }
    }
}
