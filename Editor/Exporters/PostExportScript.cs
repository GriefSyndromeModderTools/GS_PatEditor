using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Editor.Exporters
{
    [Serializable]
    public class PostExportScript
    {
        [XmlElement]
        public string BatFile { get; set; }

        [XmlElement]
        public string Directory { get; set; }

        [XmlElement]
        public bool Enabled { get; set; }

        public void Execute(Action whenFinished = null)
        {
            var th = new Thread(delegate()
            {
                if (Enabled && File.Exists(BatFile))
                {
                    Process p = new Process();
                    p.StartInfo.FileName = BatFile;
                    p.Start();

                    p.WaitForExit();
                }

                if (whenFinished != null)
                {
                    whenFinished();
                }
            });
            th.Start();
        }
    }
}
