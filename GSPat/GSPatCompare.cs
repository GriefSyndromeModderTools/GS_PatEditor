using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.GSPat
{
    //2020-05-09: write this class to compare kyouko.pat in old mods.
    class GSPatCompare
    {
        public static void Run()
        {
            var pat1 = GSPatReader.ReadFromStream(File.OpenRead(@"E:\Temp\AMLSyncDebug\20200509dld\大乱斗kyouko\kyouko.pat"));
            var pat2 = GSPatReader.ReadFromStream(File.OpenRead(@"E:\Temp\AMLSyncDebug\20200509dld\大乱斗kyouko\kyouko_dld.pat"));
            for (int i = 0; i < Math.Max(pat1.Images.Count, pat2.Images.Count); ++i)
            {
                var s1 = pat1.Images[i];
                var s2 = pat2.Images[i];
                if (s1.Substring(0, s1.Length - 3) != s2.Substring(0, s2.Length - 3))
                {
                    throw new Exception();
                }
            }
            for (int i = 0; i < Math.Max(pat1.Animations.Count, pat2.Animations.Count); ++i)
            {
                var a1 = pat1.Animations[i];
                var a2 = pat2.Animations[i];
                if (a1.AnimationID != a2.AnimationID ||
                    a1.AttackLevel != a2.AttackLevel ||
                    a1.CancelLevel != a2.CancelLevel ||
                    a1.IsLoop != a2.IsLoop ||
                    a1.Type != a2.Type ||
                    a1.CloneFrom != a2.CloneFrom ||
                    a1.CloneTo != a2.CloneTo)
                {
                    throw new Exception();
                }
                for (int j = 0; j < Math.Max(a1.Frames.Count, a2.Frames.Count); ++j)
                {
                    var f1 = a1.Frames[j];
                    var f2 = a2.Frames[j];
                    if (f1.AttackFlag != f2.AttackFlag ||
                        f1.AttackType != f2.AttackType ||
                        f1.Bind != f2.Bind ||
                        f1.Damage != f2.Damage ||
                        f1.DisplayTime != f2.DisplayTime ||
                        f1.HitG != f2.HitG ||
                        f1.HitSoundEffect != f2.HitSoundEffect ||
                        f1.HitstopOpponent != f2.HitstopOpponent ||
                        f1.HitstopSelf != f2.HitstopSelf ||
                        f1.HitVX != f2.HitVX ||
                        f1.HitVY != f2.HitVY ||
                        f1.OriginX != f2.OriginX ||
                        f1.OriginY != f2.OriginY ||
                        f1.SpriteID != f2.SpriteID ||
                        f1.StateFlag != f2.StateFlag ||
                        f1.ViewHeight != f2.ViewHeight ||
                        f1.ViewOffsetX != f2.ViewOffsetX ||
                        f1.ViewOffsetY != f2.ViewOffsetY ||
                        f1.ViewWidth != f2.ViewWidth)
                    {
                        throw new Exception();
                    }
                    for (int k = 0; k < Math.Max(f1.HitBoxes.Count, f2.HitBoxes.Count); ++k)
                    {
                        var b1 = f1.HitBoxes[k];
                        var b2 = f2.HitBoxes[k];
                        if (b1.X1 != b2.X1 ||
                            b1.Y1 != b2.Y1 ||
                            b1.X2 != b2.X2 ||
                            b1.Y2 != b2.Y2 ||
                            b1.Rotation != b2.Rotation)
                        {
                            throw new Exception();
                        }
                    }
                    for (int k = 0; k < Math.Max(f1.AttackBoxes.Count, f2.AttackBoxes.Count); ++k)
                    {
                        var b1 = f1.AttackBoxes[k];
                        var b2 = f2.AttackBoxes[k];
                        if (b1.X1 != b2.X1 ||
                            b1.Y1 != b2.Y1 ||
                            b1.X2 != b2.X2 ||
                            b1.Y2 != b2.Y2 ||
                            b1.Rotation != b2.Rotation)
                        {
                            throw new Exception();
                        }
                    }
                    {
                        var b1 = f1.PhysicsBox;
                        var b2 = f2.PhysicsBox;
                        if ((b1 == null) != (b2 == null))
                        {
                            throw new Exception();
                        }
                        if (b1 != null)
                        {
                            if (b1.X1 != b2.X1 ||
                                b1.Y1 != b2.Y1 ||
                                b1.X2 != b2.X2 ||
                                b1.Y2 != b2.Y2)
                            {
                                throw new Exception();
                            }
                        }
                    }
                }
            }
        }
    }
}
