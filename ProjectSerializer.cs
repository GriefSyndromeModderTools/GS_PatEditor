﻿using GS_PatEditor.Pat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GS_PatEditor
{
    class ProjectSerializer
    {
        private static XmlSerializer _ProjSerializer;
        public static XmlSerializer ProjSerializer
        {
            get
            {
                if (_ProjSerializer == null)
                {
                    var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(
                        t => (typeof(Effect).IsAssignableFrom(t) || typeof(Filter).IsAssignableFrom(t)) &&
                            !t.IsAbstract && t != typeof(Pat.Effects.Init.PlayerSetLabelEffect)).ToArray();
                    //var types = new Type[]
                    //{
                    //    typeof(Pat.FilteredEffect),
                    //    typeof(Pat.Effects.Init.PlayerSkillInitEffect),
                    //    typeof(Pat.SimpleListFilter),
                    //    typeof(Pat.Effects.Init.AnimationCountAfterFilter),
                    //    typeof(Pat.Effects.Init.AnimationSegmentFilter),
                    //    typeof(Pat.Effects.Init.AnimationContinueEffect),
                    //    typeof(Pat.Effects.Init.PlayerSkillIncreaseCountEffect),
                    //};
                    _ProjSerializer = new XmlSerializer(typeof(Project), types);
                }
                return _ProjSerializer;
            }
        }

        private static XmlSerializer _LocalSerializer;
        public static XmlSerializer LocalSerializer
        {
            get
            {
                if (_LocalSerializer == null)
                {
                    _LocalSerializer = new XmlSerializer(typeof(ProjectLocalInfo));
                }
                return _LocalSerializer;
            }
        }

        public static void SaveProject(Project proj, string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            using (var file = File.Open(filename, FileMode.CreateNew))
            {
                ProjSerializer.Serialize(file, proj);
            }
            var localFilename = filename + ".local";
            if (File.Exists(localFilename))
            {
                File.Delete(localFilename);
            }
            using (var file = File.Open(localFilename, FileMode.CreateNew))
            {
                LocalSerializer.Serialize(file, proj.LocalInformation);
            }
        }
        public static Project OpenProject(string filename)
        {
            Project proj;
            using (var file = File.OpenRead(filename))
            {
                proj = (Project)ProjSerializer.Deserialize(file);
            }
            if (File.Exists(filename + ".local"))
            {
                using (var file = File.OpenRead(filename + ".local"))
                {
                    proj.LocalInformation = (ProjectLocalInfo)LocalSerializer.Deserialize(file);
                }
            }
            proj.ImageList.SelectedPalette = 0;
            return proj;
        }
    }
}
