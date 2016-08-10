using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Pat
{
    public class ActionEffects
    {
        public int SegmentCount { get; private set; }
        public EffectList InitEffects;
        public EffectList UpdateEffects;
        public List<EffectList> SegmentStartEffects;
        public List<EffectList> SegmentFinishEffects;

        public ActionEffects(Action action)
        {
            SegmentCount = action.Segments.Count;

            InitEffects = new EffectList { Effects = new List<Effect>(action.InitEffects) };
            UpdateEffects = new EffectList { Effects = new List<Effect>(action.UpdateEffects) };
            SegmentStartEffects = action.SegmentStartEffects.Select(x =>
                new EffectList { Effects = new List<Effect>(x) }).ToList();
            SegmentFinishEffects = action.SegmentFinishEffects.Select(x =>
                new EffectList { Effects = new List<Effect>(x) }).ToList();
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
        }

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
}
