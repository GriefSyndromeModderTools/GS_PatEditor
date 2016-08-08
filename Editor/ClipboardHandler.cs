using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor
{
    interface ClipboardHandler
    {
        string DataID { get; }

        bool SelectedAvailable { get; }
        bool NewItemAvailabel { get; }
        bool ClipboardDataAvailable(object data);

        void New();
        object Copy();
        void Delete();
        void Paste(object data);
    }

    enum ShortcutEventType
    {
        New,
        Delete,
        Cut,
        Copy,
        Paste,
    }

    static class ClipboardHandlerExt
    {
        public static void DoNew(this ClipboardHandler h)
        {
            h.New();
        }
        public static void DoCut(this ClipboardHandler h)
        {
            var data = h.Copy();
            if (data != null)
            {
                Clipboard.SetData(h.DataID, data);
                h.Delete();
            }
        }
        public static void DoCopy(this ClipboardHandler h)
        {
            var data = h.Copy();
            if (data != null)
            {
                Clipboard.SetData(h.DataID, data);
            }
        }
        public static void DoPaste(this ClipboardHandler h)
        {
            h.Paste(Clipboard.GetData(h.DataID));
        }
        public static void DoDelete(this ClipboardHandler h)
        {
            h.Delete();
        }

        public static void ShortcutEvent(this ClipboardHandler h, ShortcutEventType type)
        {
            switch (type)
            {
                case ShortcutEventType.New:
                    h.DoNew();
                    break;
                case ShortcutEventType.Delete:
                    h.DoDelete();
                    break;
                case ShortcutEventType.Cut:
                    h.DoCut();
                    break;
                case ShortcutEventType.Copy:
                    h.DoCopy();
                    break;
                case ShortcutEventType.Paste:
                    h.DoPaste();
                    break;
            }
        }
    }
}
