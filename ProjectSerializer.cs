using GS_PatEditor.Editor;
using GS_PatEditor.Editor.Editable;
using GS_PatEditor.Editor.Exporters;
using GS_PatEditor.Localization;
using GS_PatEditor.Pat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace GS_PatEditor
{
    class SerializationBaseClassAttribute : Attribute
    {
    }

    class ProjectSerializer
    {
        private static XmlSerializer _ProjSerializer;
        private static XmlSerializer ProjSerializer
        {
            get
            {
                if (_ProjSerializer == null)
                {
                    var allTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
                    var baseClasses = allTypes
                        .Where(t => t.GetCustomAttribute<SerializationBaseClassAttribute>() != null)
                        .ToArray();
                    var types = allTypes
                        .Where(t => baseClasses.Any(tt => tt.IsAssignableFrom(t)))
                        .Where(t => !t.IsAbstract)
                        .ToArray();
                    _ProjSerializer = new XmlSerializer(typeof(Project), types);
                }
                return _ProjSerializer;
            }
        }

        private static XmlSerializer _LocalSerializer;
        private static XmlSerializer LocalSerializer
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
                SaveLocalSettings(proj, file);
            }
        }
        public static Project OpenProject(string filename)
        {
            Project proj;
            using (var file = File.OpenRead(filename))
            {
                proj = (Project)ProjSerializer.Deserialize(file);
                proj.FilePath = filename;
            }
            if (File.Exists(filename + ".local"))
            {
                using (var file = File.OpenRead(filename + ".local"))
                {
                    if (!LoadLocalSettings(proj, file))
                    {
                        return null;
                    }
                }
            }
            else
            {
                MessageBox.Show(ProgramRes.LocalNotFound,
                    EditorFormCodeRes.MsgBoxTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                if (!ShowDirectoryDialog(proj))
                {
                    return null;
                }
                proj.ImageList.ReloadPaletteList();
            }
            proj.ImageList.SelectedPalette = 0;
            return proj;
        }
        private static bool LoadLocalSettings(Project proj, FileStream file)
        {
            var local = (ProjectLocalInfo)LocalSerializer.Deserialize(file);

            proj.LastExportDirectory = local.LastExportDirectory;
            proj.LastExportFileName = local.LastExportFileName;
            proj.PostExportScript = local.PostExportScript;

            int notFound1 = 0;
            foreach (var dir in local.Directories)
            {
                var dp = proj.Settings.Directories.FirstOrDefault(d => d.Name == dir.Name);
                if (dp == null)
                {
                    ++notFound1;
                    continue;
                }
                dp.Path = dir.Path;
            }
            var notFound2 = proj.Settings.Directories.Where(d => d.Path == null);
            var notFound2c = notFound2.Count();
            if (notFound1 > 0)
            {
                MessageBox.Show(String.Format(ProgramRes.DirNotUsedFormat, notFound1),
                    EditorFormCodeRes.MsgBoxTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (notFound2c > 0)
            {
                MessageBox.Show(String.Format(ProgramRes.DirNotFoundFormat, notFound2c),
                    EditorFormCodeRes.MsgBoxTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                if (!ShowDirectoryDialog(proj))
                {
                    return false;
                }
            }
            return true;
        }
        private static void SaveLocalSettings(Project proj, FileStream file)
        {
            var local = new ProjectLocalInfo();
            local.LoadFromProject(proj);
            LocalSerializer.Serialize(file, local);
        }
        private static bool ShowDirectoryDialog(Project proj)
        {
            var dialog = new ProjectDirectoryEditForm(proj.Settings, false);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return true;
            }
            return false;
        }
    }
}
