using GS_PatEditor.Editor.Editable;
using GS_PatEditor.Editor.Exporters;
using GS_PatEditor.Editor.Exporters.CodeFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat.Effects
{
    [Serializable]
    [LocalizedClassDisplayName(typeof(FrameSinglePointProvider))]
    public class FrameSinglePointProvider : PointProvider
    {
        [XmlAttribute]
        public int Index { get; set; }

        [XmlAttribute]
        public bool AdjustSpeed { get; set; }
        public bool ShouldSerializeAdjustSpeed() { return !AdjustSpeed; }

        public FrameSinglePointProvider()
        {
            AdjustSpeed = true;
        }

        public override FramePoint GetPointForActor(Simulation.Actor actor)
        {
            if (Index < 0 || Index >= actor.CurrentFrame.Points.Count)
            {
                return new FramePoint();
            }
            var p = actor.CurrentFrame.Points[Index];
            
            float scaleX = actor.ScaleX, scaleY = actor.ScaleY;
            
            scaleX *= actor.CurrentFrame.ScaleX / 100.0f;
            scaleY *= actor.CurrentFrame.ScaleY / 100.0f;
            
            var x = scaleX * p.X;
            var y = scaleY * p.Y;

            var rx = x * Math.Cos(actor.Rotation) - y * Math.Sin(actor.Rotation);
            var ry = y * Math.Cos(actor.Rotation) + x * Math.Sin(actor.Rotation);

            //TODO should use float instead of int
            return new FramePoint
            {
                X = (int)(actor.X + rx + (AdjustSpeed ? actor.VX : 0)),
                Y = (int)(actor.Y + ry + (AdjustSpeed ? actor.VY : 0)),
            };
        }

        private static string[] pointXNames = new[] { "point0_x", "point1_x", "point2_x" };
        private static string[] pointYNames = new[] { "point0_y", "point1_y", "point2_y" };

        public override Expression GenerateX(Expression actor, GenerationEnvironment env)
        {
            var px = actor.MakeIndex(pointXNames[Index]);
            return AdjustSpeed ? new BiOpExpr(px, actor.MakeIndex("vx"), BiOpExpr.Op.Add) : px;
        }

        public override Expression GenerateY(Expression actor, GenerationEnvironment env)
        {
            var py = actor.MakeIndex(pointYNames[Index]);
            return AdjustSpeed ? new BiOpExpr(py, actor.MakeIndex("vy"), BiOpExpr.Op.Add) : py;
        }
    }
}
