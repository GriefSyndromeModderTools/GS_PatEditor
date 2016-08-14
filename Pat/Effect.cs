using GS_PatEditor.Editor.Editable;
using GS_PatEditor.Editor.Exporters;
using GS_PatEditor.Editor.Exporters.CodeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat
{
    [Serializable]
    [SerializationBaseClass]
    public abstract class Effect
    {
        public abstract void Run(Simulation.Actor actor);
        public abstract ILineObject Generate(GenerationEnvironment env);
    }

    [Serializable]
    [SerializationBaseClass]
    public abstract class Filter
    {
        public abstract bool Test(Simulation.Actor actor);
        public abstract Expression Generate(GenerationEnvironment env);
    }

    [Serializable]
    [SerializationBaseClass]
    public abstract class PointProvider
    {
        public abstract FramePoint GetPointForActor(Simulation.Actor actor);
        public abstract Expression GenerateX(Expression actor, GenerationEnvironment env);
        public abstract Expression GenerateY(Expression actor, GenerationEnvironment env);
    }

    [Serializable]
    [SerializationBaseClass]
    public abstract class Value
    {
        public abstract float Get(Simulation.Actor actor);
        public abstract Expression Generate(GenerationEnvironment env);

        public int GetInt(Simulation.Actor actor)
        {
            return (int)Get(actor);
        }
    }

    [Serializable]
    public class FilteredEffect : Effect
    {
        [XmlElement]
        [EditorChildNode("FilteredEffect_Filter")]
        public Filter Filter;
        [XmlElement]
        [EditorChildNode("FilteredEffect_Effect")]
        public Effect Effect;

        public override void Run(Simulation.Actor actor)
        {
            if (Filter.Test(actor))
            {
                Effect.Run(actor);
            }
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return new ControlBlock(ControlBlockType.If, Filter.Generate(env), new ILineObject[] {
                Effect.Generate(env),
            }).Statement();
        }
    }

    [Serializable]
    public abstract class EffectListEffect : Effect
    {
        protected abstract IEnumerable<Effect> Effects
        {
            get;
        }

        public override void Run(Simulation.Actor actor)
        {
            foreach (var effect in Effects)
            {
                effect.Run(actor);
            }
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return new SimpleBlock(Effects.Select(e => e.Generate(env)).ToArray()).Statement();
        }
    }

    [Serializable]
    public class SimpleListEffect : EffectListEffect
    {
        [XmlElement(ElementName = "Effect")]
        [EditorChildNode(null)]
        public readonly EffectList EffectList = new EffectList();

        protected override IEnumerable<Effect> Effects
        {
            get
            {
                return EffectList;
            }
        }
    }

    [Serializable]
    public abstract class FilterListFilter : Filter
    {
        protected abstract IEnumerable<Filter> Filters
        {
            get;
        }

        public override bool Test(Simulation.Actor actor)
        {
            return Filters.All(f => f.Test(actor));
        }

        public override Expression Generate(GenerationEnvironment env)
        {
            return ExpressionExt.AndAll(Filters.Select(f => f.Generate(env)).ToArray());
        }
    }

    [Serializable]
    public class SimpleListFilter : FilterListFilter
    {
        [XmlElement(ElementName = "Filter")]
        [EditorChildNode(null)]
        public readonly FilterList FilterList = new FilterList();

        public SimpleListFilter() { }

        public SimpleListFilter(params Filter[] filters)
        {
            FilterList.AddRange(filters);
        }

        protected override IEnumerable<Filter> Filters
        {
            get
            {
                return FilterList;
            }
        }
    }

    [Serializable]
    public class EffectList : IEditableList<Effect>, IEnumerable<Effect>
    {
        [XmlElement(ElementName = "Effect")]
        public List<Effect> Effects = new List<Effect>();

        public IEnumerator<Effect> GetEnumerator()
        {
            return Effects.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Effects.GetEnumerator();
        }

        public void Add(Effect effect)
        {
            Effects.Add(effect);
        }

        public void Remove(Effect val)
        {
            Effects.Remove(val);
        }

        public int FindIndex(Effect val)
        {
            return Effects.FindIndex(i => i == val);
        }

        public void Insert(int index, Effect val)
        {
            Effects.Insert(index, val);
        }

        public int Count
        {
            get
            {
                return Effects.Count;
            }
        }
    }

    [Serializable]
    public class FilterList : IEditableList<Filter>, IEnumerable<Filter>
    {
        [XmlElement(ElementName = "Filter")]
        public List<Filter> Filters = new List<Filter>();

        public IEnumerator<Filter> GetEnumerator()
        {
            return Filters.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Filters.GetEnumerator();
        }

        public void Add(Filter filter)
        {
            Filters.Add(filter);
        }

        public void AddRange(IEnumerable<Filter> filters)
        {
            Filters.AddRange(filters);
        }

        public void Remove(Filter val)
        {
            Filters.Remove(val);
        }

        public int FindIndex(Filter val)
        {
            return Filters.FindIndex(i => i == val);
        }

        public void Insert(int index, Filter val)
        {
            Filters.Insert(index, val);
        }

        public int Count
        {
            get
            {
                return Filters.Count;
            }
        }
    }

    [Serializable]
    public class ConstValue : Value
    {
        [XmlAttribute]
        public float Value { get; set; }

        public override float Get(Simulation.Actor actor)
        {
            return Value;
        }

        public override Expression Generate(GenerationEnvironment env)
        {
            return new ConstNumberExpr(Value);
        }
    }


    //tests

    [Serializable]
    public class TestEffect : Effect
    {
        public override void Run(Simulation.Actor actor)
        {
            actor.Y -= 20;
        }

        public override ILineObject Generate(GenerationEnvironment env)
        {
            return SimpleLineObject.Empty;
        }
    }

    [Serializable]
    public class TestFilter : Filter
    {
        public override bool Test(Simulation.Actor actor)
        {
            return true;
        }

        public override Expression Generate(GenerationEnvironment env)
        {
            return new ConstNumberExpr(1);
        }
    }
}
