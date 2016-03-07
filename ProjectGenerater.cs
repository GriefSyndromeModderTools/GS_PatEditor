﻿using GS_PatEditor.GSPat;
using GS_PatEditor.Pat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor
{
    class ProjectGenerater
    {
        [STAThread]
        private static void Main()
        {
            var proj = Generate();
            using (var f = File.Open(@"E:\proj.xml", FileMode.CreateNew))
            {
                Pat.PatSerialization.ProjectSerializer.Serialize(f, proj);
            }
            using (var f = File.Open(@"E:\proj_local.xml", FileMode.CreateNew))
            {
                Pat.PatSerialization.LocalSerializer.Serialize(f, proj.LocalInformation);
            }
        }
        public static Project Generate()
        {
            var patfile = OpenHomuraPat();
            if (patfile == null)
            {
                return null;
            }

            GSPatFile gspat;
            using (var file = File.OpenRead(patfile))
            {
                gspat = GSPatReader.ReadFromStream(file);
            }

            var proj = new Project();
            proj.Images = new List<FrameImage>();
            proj.Animations = new List<Pat.Animation>();
            proj.Actions = new List<Pat.Action>();
            proj.Actors = new List<Pat.Actor>();
            proj.Settings = new ProjectSettings()
            {
                ProjectName = Path.GetFileNameWithoutExtension(patfile),
                Directories = new List<ProjectDirectoryDesc>()
                {
                    new ProjectDirectoryDesc
                    {
                        Name = "images",
                        Usage = ProjectDirectoryUsage.Image,
                    }
                },
                Palettes = new List<string>()
                {
                    "palette000.pal",
                },
            };

            proj.LocalInformation = new ProjectLocalInfo
            {
                Directories = new List<ProjectDirectoryPath>()
                {
                    new ProjectDirectoryPath
                    {
                        Name = "images",
                        Path = Path.GetDirectoryName(patfile),
                    }
                },
            };

            //import animations
            ImportSimpleAnimation(proj, gspat, gspat.Animations[0], "stand");
            ImportSimpleAnimation(proj, gspat, gspat.Animations[1], "walk");

            CheckImageResources(proj, Path.GetDirectoryName(patfile));

            //refresh image cache
            proj.ImageList.SelectedPalette = 0;

            return proj;
        }
        private static void CheckImageResources(Project proj, string dir)
        {
            for (int i = 0; i < proj.Images.Count; ++i)
            {
                var res = proj.Images[i].Resource.ResourceID;
                var fullPath = Path.Combine(dir, res);
                if (File.Exists(fullPath))
                {
                    continue;
                }
                var ext = Path.GetExtension(res);
                if (ext == ".bmp")
                {
                    if (File.Exists(Path.ChangeExtension(fullPath, ".cv2")))
                    {
                        proj.Images[i].Resource.ResourceID = Path.ChangeExtension(res, ".cv2");
                        continue;
                    }
                }
                //TODO warning
            }
        }
        private static string OpenHomuraPat()
        {
            var defaultPath = @"E:\Games\[game]GRIEFSYNDROME\griefsyndrome\gs00\data\actor\homura\homura.pat";
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "homura.pat|homura.pat",

            };
            if (File.Exists(defaultPath))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(defaultPath);
            }
            if (dialog.ShowDialog() == DialogResult.Cancel)
            {
                return null;
            }
            return dialog.FileName;
        }
        private static void ImportSimpleAnimation(Pat.Project proj, GSPat.GSPatFile pat, GSPat.Animation animation, string id)
        {
            var patAnimation = new Pat.Animation
            {
                AnimationID = id,
                Segments = new List<AnimationSegment>()
                {
                    ImportSegment(proj, pat, animation),
                },
            };
            if (patAnimation.Segments.Count > 0 && patAnimation.Segments[0].Frames.Count > 0)
            {
                patAnimation.ImageID = patAnimation.Segments[0].Frames[0].ImageID;
            }
            proj.Animations.Add(patAnimation);
        }
        private static AnimationSegment ImportSegment(Pat.Project proj, GSPat.GSPatFile pat, GSPat.Animation animation)
        {
            return new AnimationSegment()
            {
                Damage = ImportDamageInfo(animation),
                CancelLevel = ImportCancelLevelEnum(animation.CancelLevel),
                IsLoop = animation.IsLoop,
                JumpCancellable = ImportCancellable(animation, 0x200000),
                SkillCancellable = ImportCancellable(animation, 0x20),
                Frames = animation.Frames.Select(f => ImportFrame(proj, pat, f)).ToList(),
            };
        }
        private static CancelLevel ImportCancelLevelEnum(short number)
        {
            switch (number)
            {
                case 0: return CancelLevel.Free;
                case 10: return CancelLevel.Light;
                case 30: return CancelLevel.Long;
                case 31: return CancelLevel.Heavy;
                case 50: return CancelLevel.Magic;
                default: return CancelLevel.None;
            }
        }
        private static Pat.Frame ImportFrame(Pat.Project proj, GSPat.GSPatFile pat, GSPat.Frame frame)
        {
            return new Pat.Frame
            {
                ImageID = AddImageToProject(proj, pat.Images[frame.SpriteID], frame).ImageID,
                OriginX = frame.OriginX,
                OriginY = frame.OriginY,
                ScaleX = frame.ImageManipulation != null ? frame.ImageManipulation.ScaleX : 100,
                ScaleY = frame.ImageManipulation != null ? frame.ImageManipulation.ScaleY : 100,
                Duration = frame.DisplayTime,
                //TODO check if rotation should be inversed
                Rotation = frame.ImageManipulation != null ? frame.ImageManipulation.Rotation : 0,
                PhysicalBox = ImportPhysicalBox(frame.PhysicsBox),
                HitBoxes = frame.HitBoxes.Select(ImportBox).ToList(),
                AttackBoxes = frame.AttackBoxes.Select(ImportBox).ToList(),
                Points = new List<FramePoint>()
                {
                    ImportPoint(frame.Point0),
                    ImportPoint(frame.Point1),
                    ImportPoint(frame.Point2),
                },
            };
        }
        private static Pat.FramePoint ImportPoint(GSPat.PointReference point)
        {
            if (point == null)
            {
                return new FramePoint { X = 0, Y = 0 };
            }
            return new FramePoint { X = point.X, Y = point.Y };
        }
        private static Pat.Box ImportBox(GSPat.Box box)
        {
            if (box == null)
            {
                return null;
            }
            return new Pat.Box
            {
                X = Math.Min(box.X1, box.X2),
                Y = Math.Min(box.Y1, box.Y2),
                W = Math.Abs(box.X1 - box.X2),
                H = Math.Abs(box.Y1 - box.Y2),
                R = -box.Rotation,
            };
        }
        private static Pat.PhysicalBox ImportPhysicalBox(GSPat.PhysicsBox box)
        {
            if (box == null)
            {
                return null;
            }
            return new PhysicalBox
            {
                X = Math.Min(box.X1, box.X2),
                Y = Math.Min(box.Y1, box.Y2),
                W = Math.Abs(box.X1 - box.X2),
                H = Math.Abs(box.Y1 - box.Y2),
            };
        }
        private static AnimationCancellableInfo ImportCancellable(GSPat.Animation animation, int mask)
        {
            var first = animation.Frames.FindIndex(f => (f.StateFlag & mask) != 0);
            if (first == -1)
            {
                return null;
            }
            return new AnimationCancellableInfo { StartFrom = animation.Frames.Take(first).Sum(f => f.DisplayTime) };
        }
        private static AnimationDamageInfo ImportDamageInfo(GSPat.Animation animation)
        {
            var atkIndex = animation.Frames.FindIndex(f => f.AttackBoxes != null && f.AttackBoxes.Count > 0);
            if (atkIndex == -1)
            {
                return null;
            }
            var atk = animation.Frames[atkIndex];
            var atkLength = animation.Frames
                .Skip(atkIndex)
                .TakeWhile(f => f.AttackBoxes == null || f.AttackBoxes.Count == 0)
                .Sum(f => f.DisplayTime);
            return new AnimationDamageInfo()
            {
                AttackType = ImportAttackTypeEnum(atk.AttackType),
                BaseDamage = atk.Damage,
                HitStop = new HitStop { Self = atk.HitstopSelf, Opponent = atk.HitstopOpponent },
                Knockback = new HitKnockback { SpeedX = atk.HitVX, SpeedY = atk.HitVY, Gravity = atk.HitG },
                SoundEffect = atk.HitSoundEffect,
                Duration = atkLength,
            };
        }
        private static AttackType ImportAttackTypeEnum(short number)
        {
            switch (number)
            {
                case 0: return AttackType.None;
                case 1: return AttackType.Light;
                case 2: return AttackType.Heavy;
                case 3: return AttackType.Smash;
                default: return AttackType.None;
            }
        }
        private static Pat.FrameImage AddImageToProject(Project proj, string filename, GSPat.Frame frame)
        {
            var find = proj.Images.FirstOrDefault(
                img => 
                    img.Resource.ResourceID == filename &&
                    img.X == frame.ViewOffsetX && img.Y == frame.ViewOffsetY &&
                    img.W == frame.ViewWidth && img.H == frame.ViewHeight
            );
            if (find != null)
            {
                return find;
            }
            var id = MakeImageID(proj, filename);
            var ret = new Pat.FrameImage()
            {
                ImageID = id,
                AlphaBlendMode = false, //not supported
                Resource = new ResourceReference { ResourceID = filename },
                X = frame.ViewOffsetX,
                Y = frame.ViewOffsetY,
                W = frame.ViewWidth,
                H = frame.ViewHeight,
            };
            proj.Images.Add(ret);
            return ret;
        }
        private static string MakeImageID(Project proj, string filename)
        {
            var defName = Path.GetFileNameWithoutExtension(filename);
            if (!proj.Images.Any(img => img.ImageID == defName))
            {
                return defName;
            }
            int i = 2;
            while (proj.Images.Any(img => img.ImageID == defName + "_" + i.ToString()))
            {
                ++i;
            }
            return defName + "_" + i.ToString();
        }
    }
}
