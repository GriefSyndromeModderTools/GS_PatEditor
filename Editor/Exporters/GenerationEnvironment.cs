using GS_PatEditor.Editor.Exporters.CodeFormat;
using GS_PatEditor.Pat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Editor.Exporters
{
    public interface GenerationEnvironment
    {
        int GetActionID(string name); //null or "" -> current action
        int GetCurrentABPostfix();
        string GenerateActionAsActorInit(string name, Action<ActionEffects> customBehaviros = null);
        string GetCurrentSkillKeyName(); //"b1"
        string GetSegmentStartEventHandlerFunctionName(); //"SegmentStartEventHandler" or null (not generated)
        void AddFunctionAlias(string newName, string oldName);
        string CreateNewFunctionName();
    }
}
