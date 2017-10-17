using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Editor.Editable
{
    class EnumDisplayNameEnumConverter : EnumConverter
    {
        private Type enumType;

        public EnumDisplayNameEnumConverter(Type type)
            : base(type)
        {
            enumType = type;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return destType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
                                         object value, Type destType)
        {
            FieldInfo fi = enumType.GetField(Enum.GetName(enumType, value));
            var dna = (LocalizedEnumDisplayNameAttribute)Attribute.GetCustomAttribute(fi,
                                        typeof(LocalizedEnumDisplayNameAttribute));
            if (dna != null)
                return dna.DisplayName;
            else
                return value.ToString();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return srcType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture,
                                           object value)
        {
            foreach (FieldInfo fi in enumType.GetFields())
            {
                var dna = (LocalizedEnumDisplayNameAttribute)Attribute.GetCustomAttribute(fi,
                                            typeof(LocalizedEnumDisplayNameAttribute));
                if ((dna != null) && ((string)value == dna.DisplayName))
                    return Enum.Parse(enumType, fi.Name);
            }
            return Enum.Parse(enumType, (string)value);
        }
    }
}
