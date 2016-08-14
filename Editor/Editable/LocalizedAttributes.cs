using GS_PatEditor.Localization.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Editor.Editable
{
    class LocalizedClassDisplayNameAttribute : DisplayNameAttribute
    {
        private static string Get(string id)
        {
            try
            {
                return ClassNameRes.ResourceManager.GetString(id);
            }
            catch
            {
                return id;
            }
        }

        public LocalizedClassDisplayNameAttribute(Type type) :
            this(type.Name)
        {
        }

        public LocalizedClassDisplayNameAttribute(string id) :
            base(Get(id))
        {
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    class LocalizedEnumDisplayNameAttribute : Attribute
    {
        private static string Get(Type cl, string val)
        {
            try
            {
                return EnumNameRes.ResourceManager.GetString(cl.Name + "_" + val);
            }
            catch
            {
                return val;
            }
        }

        public string DisplayName { get; private set; }

        public LocalizedEnumDisplayNameAttribute(Type type, string val)
        {
            DisplayName = Get(type, val);
        }
    }

    class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private static string Get(string id)
        {
            return DescriptionRes.ResourceManager.GetString(id);
        }

        public LocalizedDescriptionAttribute(string id)
            : base(Get(id))
        {
        }
    }
}
