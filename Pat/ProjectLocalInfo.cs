﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor.Pat
{
    [Serializable]
    [XmlRoot("GSPatLocal")]
    [XmlType("GSPatLocal")]
    public class ProjectLocalInfo
    {
        [XmlArray]
        [XmlArrayItem("Directory")]
        public List<ProjectDirectoryPath> Directories;

        public void LoadFromProject(Project proj)
        {
            if (Directories == null)
            {
                Directories = new List<ProjectDirectoryPath>();
            }
            else
            {
                Directories.Clear();
            }
            Directories.AddRange(proj.Settings.Directories
                .Where(d => d.Path != null && d.Path.Length > 0)
                .Select(d => new ProjectDirectoryPath { Name = d.Name, Path = d.Path }));
        }
    }
}
