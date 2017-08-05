using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Pat.Effects.Converter
{
    class ActionIDConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var obj = context.Instance;
            if (obj is IEditableEnvironment)
            {
                var env = ((IEditableEnvironment)obj).Environment;
                if (env != null)
                {
                    return new StandardValuesCollection(env.Project.Actions
                        .Select(a => a.ActionID)
                        .Where(a => a != null && a.Length != 0)
                        .OrderBy(a => a)
                        .ToArray());
                }
            }
            return new StandardValuesCollection(new string[0]);
        }
    }
    class ActionIDConverterNullable : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var obj = context.Instance;
            if (obj is IEditableEnvironment)
            {
                var env = ((IEditableEnvironment)obj).Environment;
                if (env != null)
                {
                    return new StandardValuesCollection(
                        new[] { "" }.Concat(env.Project.Actions
                            .Select(a => a.ActionID)
                            .Where(a => a != null && a.Length != 0))
                        .OrderBy(a => a)
                        .ToArray());
                }
            }
            return new StandardValuesCollection(new string[0]);
        }
    }
}
