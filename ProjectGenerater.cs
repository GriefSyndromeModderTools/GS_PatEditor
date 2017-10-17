using GS_PatEditor.GSPat;
using GS_PatEditor.Localization;
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
        public static Project GenerateEmpty(string path)
        {
            var proj = new Pat.Project()
            {
                Actions = new List<Pat.Action>(),
                Images = new List<Pat.FrameImage>(),
                Settings = new Pat.ProjectSettings
                {
                    ProjectName = ProgramRes.ProjectNameUntitled,
                    Directories = new List<Pat.ProjectDirectoryDesc>()
                        {
                            new Pat.ProjectDirectoryDesc()
                            {
                                Name = ProgramRes.DefaultImagePath,
                                Usage = Pat.ProjectDirectoryUsage.Image,
                                Path = path,
                            }
                        },
                    Palettes = new List<string>(),
                },
            };
            proj.ImageList.SelectedPalette = 0;
            return proj;
        }
        
        public static Project Generate(string patfile)
        {
            GSPatFile gspat;
            using (var file = File.OpenRead(patfile))
            {
                gspat = GSPatReader.ReadFromStream(file);
            }

            var proj = new Project();
            proj.Images = new List<FrameImage>();
            proj.Actions = new List<Pat.Action>();
            proj.Settings = new ProjectSettings()
            {
                ProjectName = Path.GetFileNameWithoutExtension(patfile),
                Directories = new List<ProjectDirectoryDesc>()
                {
                    new ProjectDirectoryDesc
                    {
                        Name = ProgramRes.DefaultImagePath,
                        Usage = ProjectDirectoryUsage.Image,
                        Path = Path.GetDirectoryName(patfile),
                    }
                },
                Palettes = new List<string>()
                {
                    "palette000.pal",
                },
            };

            //import animations
            proj.Actions.Add(new Pat.Action
            {
                ActionID = "stand",
                Segments = ImportAnimationSegments(proj, gspat, 0),
            });
            proj.Actions.Add(new Pat.Action
            {
                ActionID = "walk",
                Segments = ImportAnimationSegments(proj, gspat, 1),
            });
            proj.Actions.Add(new Pat.Action
            {
                ActionID = "attack",
                Segments = ImportAnimationSegments(proj, gspat, 20),
            });

            //set action image
            //TODO merge to import functions?
            foreach (var action in proj.Actions)
            {
                if (action.Segments.Count > 0 && action.Segments[0].Frames.Count > 0)
                {
                    action.ImageID = action.Segments[0].Frames[0].ImageID;
                }
            }

            //TODO remove this
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
        private static List<Pat.AnimationSegment> ImportAnimationSegments(Pat.Project proj, GSPat.GSPatFile pat, int index)
        {
            var start = pat.Animations.FindLastIndex(a => a.AnimationID == index + pat.Animations[0].AnimationID);
            return pat.Animations
                    .Skip(start).Take(1)
                    .Concat(pat.Animations.Skip(start + 1).TakeWhile(a => a.AnimationID == -2))
                    .Select(a => ImportSegment(proj, pat, a)).ToList();
        }
        private static List<Pat.AnimationSegment> ImportSegment(Pat.Project proj, GSPat.GSPatFile pat, int index, int segment)
        {
            var start = pat.Animations.FindLastIndex(a => a.AnimationID == index + pat.Animations[0].AnimationID);
            return pat.Animations
                    .Skip(start + segment).Take(1)
                    .Select(a => ImportSegment(proj, pat, a)).ToList();
        }
        public static AnimationSegment ImportSegment(Pat.Project proj, GSPat.GSPatFile pat, GSPat.Animation animation)
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
                case 100: return CancelLevel.Highest;
                default: return CancelLevel.None;
            }
        }
        private static Pat.Frame ImportFrame(Pat.Project proj, GSPat.GSPatFile pat, GSPat.Frame frame)
        {
            var hasIM = frame.ImageManipulation != null;
            return new Pat.Frame
            {
                ImageID = AddImageToProject(proj, pat.Images[frame.SpriteID], frame).ImageID,
                OriginX = frame.OriginX,
                OriginY = frame.OriginY,
                ScaleX = hasIM ? frame.ImageManipulation.ScaleX : 100,
                ScaleY = hasIM ? frame.ImageManipulation.ScaleY : 100,
                Alpha = hasIM ? frame.ImageManipulation.Alpha / 255.0f : 1.0f,
                Red = hasIM ? frame.ImageManipulation.Red / 255.0f : 1.0f,
                Green = hasIM ? frame.ImageManipulation.Green / 255.0f : 1.0f,
                Blue = hasIM ? frame.ImageManipulation.Blue / 255.0f : 1.0f,
                Duration = frame.DisplayTime,
                //TODO check if rotation should be inversed
                Rotation = hasIM ? frame.ImageManipulation.Rotation : 0,
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
                R = box.Rotation,
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
            return new AnimationDamageInfo()
            {
                AttackType = ImportAttackTypeEnum(atk.AttackType),
                BaseDamage = atk.Damage,
                HitStop = new HitStop { Self = atk.HitstopSelf, Opponent = atk.HitstopOpponent },
                Knockback = new HitKnockback { SpeedX = atk.HitVX, SpeedY = atk.HitVY, Gravity = atk.HitG },
                SoundEffect = atk.HitSoundEffect,
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
        //TODO move to image list
        public static Pat.FrameImage AddImageToProject(Project proj, string filename, GSPat.Frame frame)
        {
            if (Path.GetExtension(filename) == ".bmp" || Path.GetExtension(filename) == ".png")
            {
                filename = Path.ChangeExtension(filename, ".cv2");
            }

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
                //TODO AlphaBlend might be 0,1,2. catch 2
                AlphaBlendMode = frame.ImageManipulation == null ? false : frame.ImageManipulation.AlphaBlend == 1,
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
