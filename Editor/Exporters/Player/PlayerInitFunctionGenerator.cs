using GS_PatEditor.Editor.Exporters.CodeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Editor.Exporters.Player
{
    class PlayerInitFunctionGenerator
    {
        public static void Generate(PlayerExporter exporter, Pat.Project proj, CodeGenerator writer)
        {
            var stringtype = String.IsNullOrEmpty(exporter.PlayerStringType) ? exporter.PlayerName : exporter.PlayerStringType;
            var func = new FunctionBlock(
                "Init_" + exporter.PlayerName,
                new[] { "t" },
                new ILineObject[] {
                    new SimpleLineObject("this.type = " + exporter.PlayerIntType + ";"),
                    new SimpleLineObject("this.fontType = " + exporter.PlayerFontType + ";"),
                    new SimpleLineObject("this.PlayerInit(\"" + stringtype + "\", t.playerID);"),
                    new SimpleLineObject("this.CompileFile(\"data/actor/" + exporter.ScriptFileName + ".nut\", this.u);"),
                    new SimpleLineObject("this.u.CA = " + exporter.BaseIndex + ";"),
                    new SimpleLineObject("this.u.regainCycle = " + exporter.PlayerInformation.RegainCycle+ ";"),
                    new SimpleLineObject("this.u.regainRate = " + exporter.PlayerInformation.RegainRate + ";"),
                    new SimpleLineObject("this.u.magicUse = " + exporter.PlayerInformation.MagicUse + ";"),
                    new SimpleLineObject("this.u.atkOffset = " + exporter.PlayerInformation.AtkOffset + ";"),
                    new SimpleLineObject("this.SetMotion(this.u.CA + 0, 0);"),
                    new SimpleLineObject("this.SetUpdateFunction(this.u.Update_Normal);"),
                    new SimpleLineObject("this.fallLabel = this.u.BeginFall;"),
                    new SimpleLineObject("this.u.dexLV = " + exporter.PlayerInformation.RushCount + ";"),
                    new SimpleLineObject("this.world2d.CreateActor(this.x, this.y, this.direction, this.Player_Shadow, this.DefaultShotTable());"),
                });
            writer.WriteStatement(new BlockStatement(func));
        }
    }
}
