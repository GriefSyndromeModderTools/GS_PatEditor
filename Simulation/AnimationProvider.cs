using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Simulation
{
    public interface AnimationProvider
    {
        Pat.Action GetActionByID(string id);
        AnimationProvider SetDefault(Pat.Action action);
    }
}
