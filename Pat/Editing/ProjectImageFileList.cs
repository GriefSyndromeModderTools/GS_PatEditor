using GS_PatEditor.Images;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Pat.Editing
{
    public class ProjectImageFileList : IDisposable
    {
        private readonly Project _Project;

        private int _SelectedPalette;
        public int SelectedPalette
        {
            get
            {
                if (_Project.Settings.Palettes.Count == 0)
                {
                    _SelectedPalette = -1;
                }
                else if (_SelectedPalette >= _Project.Settings.Palettes.Count)
                {
                    _SelectedPalette = 0;
                }
                return _SelectedPalette;
            }
            set
            {
                if (_Project.Settings.Palettes.Count == 0)
                {
                    _SelectedPalette = -1;
                }
                else if (value >= _Project.Settings.Palettes.Count)
                {
                    _SelectedPalette = 0;
                }
                else
                {
                    _SelectedPalette = value;
                }
                OnPaletteChange();
            }
        }

        private Color[] _Palette;

        private readonly Dictionary<string, LoadedFrameImage> cachedImage = new Dictionary<string, LoadedFrameImage>();
        private readonly LoadedFrameImage _EmptyImage = new LoadedFrameImage { Bitmap = null, UsePalette = false };

        private readonly Dictionary<string, AbstractImage> cachedResource = new Dictionary<string, AbstractImage>();

        private readonly Dictionary<string, Bitmap> cachedUnclipped = new Dictionary<string, Bitmap>();

        public ProjectImageFileList(Project proj)
        {
            _Project = proj;
        }

        public Bitmap GetImage(string id)
        {
            LoadedFrameImage ret;
            if (cachedImage.TryGetValue(id, out ret))
            {
                return ret.Bitmap;
            }

            var imgDesc = _Project.Images.FirstOrDefault(f => f.ImageID == id);
            ret = imgDesc == null ? _EmptyImage : LoadImage(imgDesc);
            cachedImage.Add(id, ret);

            return ret.Bitmap;
        }

        public Texture GetTexture(string id, Render.RenderEngine re)
        {
            LoadedFrameImage ret;
            if (cachedImage.TryGetValue(id, out ret))
            {
                if (ret.Bitmap == null)
                {
                    return null;
                }
                if (ret.Texture == null)
                {
                    ret.Texture = re.CreateTextureFromBitmap(ret.Bitmap);
                }
                return ret.Texture;
            }

            var imgDesc = _Project.Images.FirstOrDefault(f => f.ImageID == id);
            ret = imgDesc == null ? _EmptyImage : LoadImage(imgDesc);
            cachedImage.Add(id, ret);

            if (ret.Bitmap != null)
            {
                ret.Texture = re.CreateTextureFromBitmap(ret.Bitmap);
            }
            return ret.Texture;
        }

        private LoadedFrameImage LoadImage(FrameImage image)
        {
            var res = _Project.FindResource(ProjectDirectoryUsage.Image, image.Resource.ResourceID);
            if (res != null)
            {
                AbstractImage imageData = LoadResource(res);
                if (imageData == null)
                {
                    return _EmptyImage;
                }
                var clipped = ClipBitmap(imageData, image);

                if (image.AlphaBlendMode)
                {
                    AbstractImage.MakeAlphaBlendBitmap(clipped);
                }

                return new LoadedFrameImage
                {
                    Bitmap = clipped,
                    UsePalette = imageData.UsePalette
                };
            }
            return _EmptyImage;
        }

        public Bitmap GetImageUnclippedByRes(string id, bool alphaBlend)
        {
            var res = _Project.FindResource(ProjectDirectoryUsage.Image, id);
            if (res == null)
            {
                return null;
            }

            Bitmap ret;
            AbstractImage imageData = LoadResource(res);
            if (imageData == null)
            {
                return null;
            }
            ret = imageData.ToBitmap(_Palette, new Rectangle(0, 0, imageData.Width, imageData.Height));

            if (alphaBlend)
            {
                AbstractImage.MakeAlphaBlendBitmap(ret);
            }
            return ret;
        }

        public Bitmap GetImageUnclipped(string id)
        {
            Bitmap ret;
            if (cachedUnclipped.TryGetValue(id, out ret))
            {
                return ret;
            }

            var imgDesc = _Project.Images.FirstOrDefault(f => f.ImageID == id);
            if (imgDesc == null)
            {
                return null;
            }
            ret = GetImageUnclippedByRes(imgDesc.Resource.ResourceID, imgDesc.AlphaBlendMode);
            cachedUnclipped.Add(id, ret);
            return ret;
        }

        private AbstractImage LoadResource(string res)
        {
            AbstractImage ret;
            if (cachedResource.TryGetValue(res, out ret))
            {
                return ret;
            }
            
            if (Path.GetExtension(res) == ".dds")
            {
                ret = new DDSImage(res);
            }
            else if (Path.GetExtension(res) == ".cv2")
            {
                ret = new CV2Image(res);
            }

            //add it even if it's null (avoid keep trying)
            cachedResource.Add(res, ret);

            return ret;
        }

        private Bitmap ClipBitmap(AbstractImage res, FrameImage img)
        {
            return res.ToBitmap(_Palette, new Rectangle(img.X, img.Y, img.W, img.H));
        }

        private void OnPaletteChange()
        {
            if (SelectedPalette == -1)
            {
                _Palette = null;
                return;
            }
            string palName = null;
            if (_Project.Settings.Palettes[SelectedPalette] != null)
            {
                palName = _Project.FindResource(ProjectDirectoryUsage.Image, _Project.Settings.Palettes[SelectedPalette]);
            }
            if (palName == null)
            {
                _Palette = null;
                return;
            }
            _Palette = CV2Palette.ReadPaletteFile(palName);

            ClearFrameImages(true);
        }

        public void ResetImage(FrameImage img)
        {
            cachedImage.Remove(img.ImageID);
            cachedUnclipped.Remove(img.ImageID);
        }

        private List<string> _ToRemove = new List<string>();
        private void ClearFrameImages(bool palletteOnly)
        {
            _ToRemove.Clear();

            foreach (var img in cachedImage)
            {
                if (!img.Value.UsePalette && palletteOnly)
                {
                    continue;
                }
                _ToRemove.Add(img.Key);

                var bitmap = img.Value.Bitmap;
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }
                var texture = img.Value.Texture;
                if (texture != null)
                {
                    texture.Dispose();
                }
            }

            foreach (var key in _ToRemove)
            {
                cachedImage.Remove(key);
            }
        }

        private void ClearResources()
        {
            foreach (var res in cachedResource)
            {
                if (res.Value != null)
                {
                    res.Value.Dispose();
                }
            }
            cachedResource.Clear();
            foreach (var res in cachedUnclipped)
            {
                if (res.Value != null)
                {
                    res.Value.Dispose();
                }
            }
            cachedUnclipped.Clear();
        }

        public void Dispose()
        {
            ReloadAllResources();
        }

        public void ReloadAllResources()
        {
            ClearFrameImages(false);
            ClearResources();
            SelectedPalette = 0;
        }

        public void ReloadPaletteList()
        {
            var result = new List<string>();
            foreach (var dir in _Project.Settings.Directories
                .Where(d => d.Usage == ProjectDirectoryUsage.Image))
            {
                if (Directory.Exists(dir.Path))
                {
                    var palList = System.IO.Directory.EnumerateFiles(dir.Path,
                        "*.pal", System.IO.SearchOption.TopDirectoryOnly)
                        .Select(file => System.IO.Path.GetFileName(file));
                    result.AddRange(palList);
                }
            }
            result.Sort();
            _Project.Settings.Palettes = result;
        }
    }
}
