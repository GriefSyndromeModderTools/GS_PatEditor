using GS_PatEditor.Editor.Exporters.CodeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Pat.Effects
{
    static class ActorVariableHelper
    {
        public static FunctionBlock GenerateSharedFunction()
        {
            return new FunctionBlock("SYS_GetActorVariableHelper", new[] { "name" }, new ILineObject[] {
                new SimpleLineObject("if (!(\"variables\" in this.u)) this.u.variables <- {};"),
                new SimpleLineObject("if (!(name in this.u.variables)) this.u.variables[name] <- 0;"),
                new SimpleLineObject("return this.u.variables[name];"),
            });
        }

        public static ILineObject GenerateSet(string name, Expression expr)
        {
            var nname = name.Replace("\\", "\\\\").Replace("\"", "\\\\");
            return new SimpleBlock(new ILineObject[] {
                new SimpleLineObject("if (!(\"variables\" in this.u)) this.u.variables <- {};"),
                new SimpleLineObject("if (!(\"" + nname + "\" in this.u.variables)) this.u.variables[\"" + nname + "\"] <- 0;"),
                ThisExpr.Instance.MakeIndex("u").MakeIndex("variables").MakeIndex(name).Assign(expr).Statement(),
            }).Statement();
        }

        public static Expression GenerateGet(string name)
        {
            return ThisExpr.Instance.MakeIndex("u").MakeIndex("uu").MakeIndex("uuu").MakeIndex("SYS_GetActorVariableHelper")
                .MakeIndex("call").Call(ThisExpr.Instance, new ConstStringExpr(name));
        }

        public static ILineObject GenerateSet(Expression actor, string name, Expression expr)
        {
            var nname = name.Replace("\\", "\\\\").Replace("\"", "\\\\");
            return new SimpleBlock(new ILineObject[] {
                new LocalVarStatement("actor_variable_actor_object", actor),
                new SimpleLineObject("if (!(\"variables\" in actor_variable_actor_object.u)) actor_variable_actor_object.u.variables <- {};"),
                new SimpleLineObject("if (!(\"" + nname + "\" in actor_variable_actor_object.u.variables)) actor_variable_actor_object.u.variables[\"" + nname + "\"] <- 0;"),
                new IdentifierExpr("actor_variable_actor_object").MakeIndex("u").MakeIndex("variables").MakeIndex(name).Assign(expr).Statement(),
            }).Statement();
        }

        public static Expression GenerateGet(Expression actor, string name)
        {
            return ThisExpr.Instance.MakeIndex("u").MakeIndex("uu").MakeIndex("uuu").MakeIndex("SYS_GetActorVariableHelper")
                .MakeIndex("call").Call(actor, new ConstStringExpr(name));
        }
    }
}
