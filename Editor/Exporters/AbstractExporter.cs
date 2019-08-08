using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS_PatEditor.Editor.Exporters
{
    [Serializable]
    [SerializationBaseClass]
    public abstract class AbstractExporter
    {
        private Pat.Project _Project;
        private string _OutputFile;
        private string _OutputDir;

        private ImageListExporter _ImageList;
        private GSPat.GSPatFile _OutputData;
        private List<CodeGenerator> _Codes = new List<CodeGenerator>();

        public abstract void Export(Pat.Project proj);
        public abstract void ShowOptionDialog(Pat.Project proj);

        public void InitExporter(Pat.Project proj, string outputFile)
        {
            _Project = proj;
            _OutputFile = outputFile;
            _OutputDir = Path.GetDirectoryName(outputFile);

            _ImageList = new ImageListExporter();
            _OutputData = new GSPat.GSPatFile()
            {
                Animations = new List<GSPat.Animation>(),
                Images = new List<string>(),
            };
            _Codes.Clear();

            //first export images
            foreach (var img in proj.Images)
            {
                _ImageList.AddImage(_OutputData, img);
            }
        }

        public void FinishExporter()
        {
            //save pat
            if (System.IO.File.Exists(_OutputFile))
            {
                System.IO.File.Delete(_OutputFile);
            }
            using (var stream = System.IO.File.Open(_OutputFile, System.IO.FileMode.CreateNew))
            {
                using (var writer = new System.IO.BinaryWriter(stream))
                {
                    GSPat.GSPatWriter.Write(_OutputData, writer);
                }
            }

            //save codes
            foreach (var code in _Codes)
            {
                code.Finish();
            }

            //reset referenced objects
            _Codes.Clear();
            _Project = null;
            _ImageList = null;
            _OutputData = null;
        }

        protected void AddNormalAnimation(Pat.Action action, int id)
        {
            ExportHelper.ExportAnimation(_OutputData, _ImageList, action, id);
        }

        protected void AddCloneAnimation(int fromID, int toID)
        {
            _OutputData.Animations.Add(new GSPat.Animation()
            {
                AnimationID = -1,
                Type = GSPat.AnimationType.Clone,
                CloneFrom = (short)fromID,
                CloneTo = (short)toID,
            });
        }

        protected CodeGenerator AddCodeFile(string filename)
        {
            var ret = new CodeGenerator(Path.Combine(_OutputDir, filename));
            _Codes.Add(ret);
            return ret;
        }

        protected void ExchangeAnimationOrder()
        {
            ExportHelper.ReorderAnimation(_OutputData);
        }

        public virtual bool IsImageIDDirectlyUsed(string imageID)
        {
            return false;
        }
    }
}
