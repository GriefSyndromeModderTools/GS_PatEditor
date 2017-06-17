using GS_PatEditor.Pat.Effects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace GS_PatEditor.Editor.Exporters.Player
{
    class ImageIDEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context,
            IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService svc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            string str = value as string;
            var obj = context.Instance as IEditableEnvironment;
            if (svc != null && obj != null)
            {
                using (var dialog = new ImageSelectForm(obj.Environment.Project, false))
                {
                    dialog.SelectedImage = str;
                    if (svc.ShowDialog(dialog) == DialogResult.OK)
                    {
                        return dialog.SelectedImage;
                    }
                }
            }
            return value;
        }
    }
}
