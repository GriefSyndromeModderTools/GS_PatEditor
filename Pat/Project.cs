using GS_PatEditor.Editor.Exporters;
using GS_PatEditor.Pat.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat
{
    [Serializable]
    [XmlRoot("GSPatProject")]
    [XmlType("GSPatProject")]
    public class Project
    {
        [XmlElement(ElementName = "Settings")]
        public ProjectSettings Settings;

        [XmlArray]
        public List<FrameImage> Images;

        [XmlArray]
        public List<Action> Actions;

        [XmlElement]
        public AbstractExporter Exporter;

        [XmlIgnore]
        public readonly ProjectImageFileList ImageList;

        [XmlIgnore]
        public PostExportScript PostExportScript;

        [XmlIgnore]
        public string FilePath;

        [XmlIgnore]
        public bool IsEmptyProject;

        [XmlIgnore]
        public string LastExportDirectory;

        [XmlIgnore]
        public string LastExportFileName;

        public Project()
        {
            ImageList = new ProjectImageFileList(this);
            PostExportScript = new PostExportScript();
        }
    }
}
