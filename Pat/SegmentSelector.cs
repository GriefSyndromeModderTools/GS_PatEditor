using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat
{
    [Serializable]
    [TypeConverter(typeof(SegmentSelectorTypeConverter))]
    public class SegmentSelector
    {
        [XmlElement(ElementName = "Segment")]
        public readonly List<int> IndexList = new List<int>();

        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsReversed { get; set; }

        [XmlIgnore]
        public string Index
        {
            get
            {
                if (IsReversed && IndexList.Count == 0)
                {
                    return "*";
                }
                return (IsReversed ? "*," : "") + String.Join(",", IndexList);
            }
            set
            {
                var lastReversed = IsReversed;

                if (value == null || value.Length == 0)
                {
                    IsReversed = false;
                    IndexList.Clear();
                    return;
                }
                if (value == "*")
                {
                    IsReversed = true;
                    IndexList.Clear();
                    return;
                }
                if (value.StartsWith("*,"))
                {
                    value = value.Substring(2);
                    IsReversed = true;
                }
                else
                {
                    IsReversed = false;
                }

                var list = value.Split(',');
                var listInt = new List<int>();
                foreach (var i in list)
                {
                    int ii;
                    if (Int32.TryParse(i, out ii) && ii >= 0)
                    {
                        listInt.Add(ii);
                    }
                    else
                    {
                        //failed
                        IsReversed = lastReversed;
                        return;
                    }
                }
                IndexList.Clear();
                IndexList.AddRange(listInt);
            }
        }
    }

    public class SegmentSelectorTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                return new SegmentSelector { Index = (string)value };
            }
            return base.ConvertFrom(context, culture, value);
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
            {
                return "*";
            }
            if (value is SegmentSelector && destinationType == typeof(string))
            {
                return ((SegmentSelector)value).Index;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public static class SegmentSelectorHelper
    {
        public static void MakeEffectsAsUpdate(ActionEffects dest, SegmentSelector ss, Effect effect)
        {
            IEnumerable<int> segments;
            if (ss.IsReversed)
            {
                segments = Enumerable.Range(0, dest.SegmentCount).Except(ss.IndexList);
            }
            else
            {
                segments = ss.IndexList;
            }
            foreach (var s in segments)
            {
                dest.UpdateEffects.Add(new FilteredEffect
                {
                    Filter = new Effects.AnimationSegmentFilter { Segment = s },
                    Effect = effect,
                });
            }
        }

        public static void MakeEffectsAsStart(ActionEffects dest, SegmentSelector ss, Effect effect)
        {
            IEnumerable<int> segments;
            if (ss.IsReversed)
            {
                segments = Enumerable.Range(0, dest.SegmentCount).Except(ss.IndexList);
            }
            else
            {
                segments = ss.IndexList;
            }
            foreach (var s in segments)
            {
                dest.SegmentStartEffects.AddEffectToList(s, effect);
            }
        }
    }
}
