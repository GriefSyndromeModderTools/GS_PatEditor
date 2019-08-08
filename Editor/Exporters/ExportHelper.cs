using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Editor.Exporters
{
    static class ExportHelper
    {
        public static void ReorderAnimation(GSPat.GSPatFile file)
        {
            List<int> oldIndex = new List<int>();
            for (int i = 0; i < file.Animations.Count; ++i)
            {
                var a = file.Animations[i];
                if (a.AnimationID != -2)
                {
                    oldIndex.Add(i);
                }
            }
            var r = new Random();
            var reorder = Enumerable.Range(0, oldIndex.Count)
                .Select(ii => new { Index = ii, Rand = r.NextDouble() })
                .OrderBy(gg => gg.Rand)
                .Select(gg => gg.Index)
                .Select(ii => oldIndex[ii])
                .ToArray();
            //reorder = oldIndex.ToArray();
            var newList = new List<GSPat.Animation>();
            foreach (var index in reorder)
            {
                int i = index;
                do
                {
                    newList.Add(file.Animations[i]);
                } while (++i < file.Animations.Count && file.Animations[i].AnimationID == -2);
            }
            file.Animations = newList;
        }

        public static void ExportAnimation(GSPat.GSPatFile file, ImageListExporter images,
            Pat.Action action, int id)
        {
            {
                var eAnimation = new GSPat.Animation()
                {
                    AnimationID = id,
                    AttackLevel = 0,
                    CancelLevel = ExportCancelLevel(action.Segments[0].CancelLevel),
                    IsLoop = action.Segments[0].IsLoop,
                    Type = GSPat.AnimationType.Normal,
                };
                ExportFrames(eAnimation, action.Segments[0], images);
                file.Animations.Add(eAnimation);
            }

            for (int i = 1; i < action.Segments.Count; ++i)
            {
                var eAnimation = new GSPat.Animation()
                {
                    AnimationID = -2,
                    AttackLevel = 0,
                    CancelLevel = ExportCancelLevel(action.Segments[i].CancelLevel),
                    IsLoop = action.Segments[i].IsLoop,
                    Type = GSPat.AnimationType.Normal,
                };
                ExportFrames(eAnimation, action.Segments[i], images);
                file.Animations.Add(eAnimation);
            }
        }

        public static short ExportCancelLevel(Pat.CancelLevel value)
        {
            switch (value)
            {
                case Pat.CancelLevel.Free:
                case Pat.CancelLevel.None:
                    return 0;
                case Pat.CancelLevel.Light:
                    return 10;
                case Pat.CancelLevel.Long:
                    return 30;
                case Pat.CancelLevel.Heavy:
                    return 31;
                case Pat.CancelLevel.Magic:
                    return 50;
                case Pat.CancelLevel.Highest:
                    return 100;
            }
            return 0;
        }

        public static void ExportFrames(GSPat.Animation toList, Pat.AnimationSegment fromList,
            ImageListExporter images)
        {
            toList.Frames = new List<GSPat.Frame>();
            var attackType = fromList.Damage == null ? Pat.AttackType.None : fromList.Damage.AttackType;
            var damage = fromList.Damage == null ? 0 : fromList.Damage.BaseDamage;
            var hitG = fromList.Damage == null ? 0 : fromList.Damage.Knockback.Gravity;
            var se = fromList.Damage == null ? 0 : fromList.Damage.SoundEffect;
            var hso = fromList.Damage == null ? 0 : fromList.Damage.HitStop.Opponent;
            var hss = fromList.Damage == null ? 0 : fromList.Damage.HitStop.Self;
            var hitX = fromList.Damage == null ? 0 : fromList.Damage.Knockback.SpeedX;
            var hitY = fromList.Damage == null ? 0 : fromList.Damage.Knockback.SpeedY;
            var cancelJump = fromList.JumpCancellable == null ? Int32.MaxValue : fromList.JumpCancellable.StartFrom;
            var cancelSkill = fromList.SkillCancellable == null ? Int32.MaxValue : fromList.SkillCancellable.StartFrom;

            var frameTime = 0;
            foreach (var frame in fromList.Frames)
            {
                var img = images.GetImage(frame.ImageID);

                var eFrame = new GSPat.Frame()
                {
                    AttackBoxes = ExportBoxs(frame.AttackBoxes),
                    AttackFlag = 0,
                    AttackType = (short)attackType,
                    Bind = 0,
                    Damage = (short)damage,
                    DisplayTime = (short)frame.Duration,
                    HitBoxes = ExportBoxs(frame.HitBoxes),
                    HitG = (short)hitG,
                    HitSoundEffect = (short)se,
                    HitstopOpponent = (short)hso,
                    HitstopSelf = (short)hss,
                    HitVX = (short)hitX,
                    HitVY = (short)hitY,

                    //TODO
                    ImageManipulation = ExportIM(frame, img),

                    OriginX = (short)frame.OriginX,
                    OriginY = (short)frame.OriginY,
                    PhysicsBox = ExportPhysical(frame.PhysicalBox),
                    Point0 = ExportPoint(frame.Points, 0),
                    Point1 = ExportPoint(frame.Points, 1),
                    Point2 = ExportPoint(frame.Points, 2),
                    SpriteID = images.GetImageIntID(frame.ImageID),
                    StateFlag = (frameTime >= cancelSkill ? 0x20 : 0) + (frameTime >= cancelJump ? 0x200000 : 0),
                    ViewHeight = (short)img.H,
                    ViewOffsetX = (short)img.X,
                    ViewOffsetY = (short)img.Y,
                    ViewWidth = (short)img.W,
                };
                toList.Frames.Add(eFrame);

                frameTime += frame.Duration;
            }
        }

        public static GSPat.PointReference ExportPoint(List<Pat.FramePoint> list, int index)
        {
            if (list == null || index >= list.Count)
            {
                return new GSPat.PointReference();
            }
            return new GSPat.PointReference
            {
                X = (short)list[index].X,
                Y = (short)list[index].Y,
            };
        }

        public static List<GSPat.Box> ExportBoxs(List<Pat.Box> boxes)
        {
            return boxes.Select(b => new GSPat.Box()
            {
                X1 = b.X,
                X2 = b.X + b.W,
                Y1 = b.Y,
                Y2 = b.Y + b.H,
                Rotation = b.R,
            }).ToList();
        }

        public static GSPat.PhysicsBox ExportPhysical(Pat.PhysicalBox box)
        {
            if (box == null)
            {
                return null;
            }
            return new GSPat.PhysicsBox()
            {
                X1 = box.X,
                X2 = box.X + box.W,
                Y1 = box.Y,
                Y2 = box.Y + box.H,
            };
        }

        private static GSPat.ImageManipulation ExportIM(Pat.Frame frame, Pat.FrameImage image)
        {
            var scaleX = frame.ScaleX;
            var scaleY = frame.ScaleY;
            var rotation = frame.Rotation;
            var alphaBlend = image.AlphaBlendMode;
            //TODO add other fields
            if (scaleX == 100 && scaleY == 100 && rotation == 0 && !alphaBlend)
            {
                return null;
            }
            return new GSPat.ImageManipulation
            {
                AlphaBlend = (short)(alphaBlend ? 1 : 0),
                ScaleX = (short)scaleX,
                ScaleY = (short)scaleY,
                Rotation = (short)rotation,

                Alpha = (byte)(255 * frame.Alpha),
                Red = (byte)(255 * frame.Red),
                Green = (byte)(255 * frame.Green),
                Blue = (byte)(255 * frame.Blue),
            };
        }
    }
}
