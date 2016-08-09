using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Editor.Editable
{
    interface IDataNodeDisplayNameProvider
    {
        bool OverrideFullName { get; }
        string DisplayName { get; }
    }
}
