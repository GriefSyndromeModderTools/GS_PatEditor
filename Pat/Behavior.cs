using GS_PatEditor.Editor.Editable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat
{
    public class ActionEffects
    {
        private Project _Project;
        private Simulation.ActionProvider _ActionProvider;
        public int SegmentCount { get; private set; }

        public EffectList InitEffects;
        public EffectList PostInitEffects;
        public EffectList UpdateEffects;
        public EffectList PostUpdateEffects;

        public List<EffectList> SegmentStartEffects;
        public List<EffectList> SegmentFinishEffects;

        public Action GetActionByName(string name)
        {
            if (_Project != null)
            {
                return _Project.Actions.FirstOrDefault(a => a.ActionID == name);
            }
            if (_ActionProvider != null)
            {
                return _ActionProvider.GetActionByID(name);
            }
            return null;
        }

        public ActionEffects(Simulation.ActionProvider provider, Action action)
        {
            _ActionProvider = provider;
            SegmentCount = action.Segments.Count;

            InitEffects = new EffectList { Effects = new List<Effect>() };
            PostInitEffects = new EffectList { Effects = new List<Effect>() };
            UpdateEffects = new EffectList { Effects = new List<Effect>() };
            PostUpdateEffects = new EffectList { Effects = new List<Effect>() };
            SegmentStartEffects = new List<EffectList>();
            SegmentFinishEffects = new List<EffectList>();
        }

        public ActionEffects(Project proj, Action action)
        {
            _Project = proj;
            SegmentCount = action.Segments.Count;

            InitEffects = new EffectList { Effects = new List<Effect>() };
            PostInitEffects = new EffectList { Effects = new List<Effect>() };
            UpdateEffects = new EffectList { Effects = new List<Effect>() };
            PostUpdateEffects = new EffectList { Effects = new List<Effect>() };
            SegmentStartEffects = new List<EffectList>();
            SegmentFinishEffects = new List<EffectList>();
        }

        public void ProcessPostEffects()
        {
            UpdateEffects.Effects.AddRange(PostUpdateEffects.Effects);
            PostUpdateEffects.Effects.Clear();
            InitEffects.Effects.AddRange(PostInitEffects.Effects);
            PostInitEffects.Effects.Clear();
        }
    }

    public static class EffectListListExt
    {
        public static void AddEffectToList(this List<EffectList> list, int index, Effect effect)
        {
            while (list.Count <= index)
            {
                list.Add(new EffectList());
            }
            list[index].Add(effect);
        }

        public static void InsertEffectToList(this List<EffectList> list, int index, Effect effect)
        {
            while (list.Count <= index)
            {
                list.Add(new EffectList());
            }
            list[index].Insert(0, effect);
        }
    }

    [Serializable]
    [SerializationBaseClass]
    public abstract class Behavior
    {
        public Behavior()
        {
            _NextID = GetNextID();
            Enabled = true;
        }

        [XmlAttribute]
        [DefaultValue(true)]
        [LocalizedDescriptionAttribute("Behavior_Enabled")]
        public bool Enabled { get; set; }

        private static int _NextID = 1;
        private static object _Lock = new object();

        private static int GetNextID()
        {
            lock (_Lock)
            {
                return _NextID++;
            }
        }

        protected int UniqueID
        {
            get
            {
                return _NextID;
            }
        }

        protected string GetVariableName(string name)
        {
            return "__bhv" + UniqueID.ToString() + "_" + name;
        }

        public abstract void MakeEffects(ActionEffects effects);
    }

    [Serializable]
    public class BehaviorList : IEditableList<Behavior>, IEnumerable<Behavior>
    {
        [XmlElement(ElementName = "Behavior")]
        public List<Behavior> Behaviors = new List<Behavior>();

        public IEnumerator<Behavior> GetEnumerator()
        {
            return Behaviors.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Behaviors.GetEnumerator();
        }

        public void Add(Behavior filter)
        {
            Behaviors.Add(filter);
        }

        public void AddRange(IEnumerable<Behavior> filters)
        {
            Behaviors.AddRange(filters);
        }

        public void Remove(Behavior val)
        {
            Behaviors.Remove(val);
        }

        public int FindIndex(Behavior val)
        {
            return Behaviors.FindIndex(i => i == val);
        }

        public void Insert(int index, Behavior val)
        {
            Behaviors.Insert(index, val);
        }

        public int Count
        {
            get
            {
                return Behaviors.Count;
            }
        }
    }
}
