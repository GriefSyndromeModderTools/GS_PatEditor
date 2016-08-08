using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor
{
    interface IClipboardUIElement
    {
        bool Enabled { set; }
        event EventHandler Click;
    }
    class ClipboardUIElementToolstripItem : IClipboardUIElement
    {
        private readonly ToolStripItem _Control;

        public ClipboardUIElementToolstripItem(ToolStripItem ctrl)
        {
            _Control = ctrl;
            ctrl.Click += delegate(object sender, EventArgs e)
            {
                if (Click != null)
                {
                    Click(sender, e);
                }
            };
        }

        public bool Enabled
        {
            set { _Control.Enabled = value; }
        }

        public event EventHandler Click;
    }

    class ClipboardUIProvider
    {
        private readonly ClipboardHandler _Handler;

        public ClipboardUIProvider(ClipboardHandler handler)
        {
            _Handler = handler;
        }

        private IClipboardUIElement _New;
        public IClipboardUIElement New
        {
            get
            {
                return _New;
            }
            set
            {
                if (_New != null)
                {
                    _New.Click -= New_Click;
                }
                if (value != null)
                {
                    value.Click += New_Click;
                }
                _New = value;
            }
        }

        private IClipboardUIElement _Cut;
        public IClipboardUIElement Cut
        {
            get
            {
                return _Cut;
            }
            set
            {
                if (_Cut != null)
                {
                    _Cut.Click -= Cut_Click;
                }
                if (value != null)
                {
                    value.Click += Cut_Click;
                }
                _Cut = value;
            }
        }

        private IClipboardUIElement _Copy;
        public IClipboardUIElement Copy
        {
            get
            {
                return _Copy;
            }
            set
            {
                if (_Copy != null)
                {
                    _Copy.Click -= Copy_Click;
                }
                if (value != null)
                {
                    value.Click += Copy_Click;
                }
                _Copy = value;
            }
        }

        private IClipboardUIElement _Paste;
        public IClipboardUIElement Paste
        {
            get
            {
                return _Paste;
            }
            set
            {
                if (_Paste != null)
                {
                    _Paste.Click -= Paste_Click;
                }
                if (value != null)
                {
                    value.Click += Paste_Click;
                }
                _Paste = value;
            }
        }

        private IClipboardUIElement _Delete;
        public IClipboardUIElement Delete
        {
            get
            {
                return _Delete;
            }
            set
            {
                if (_Delete != null)
                {
                    _Delete.Click -= Delete_Click;
                }
                if (value != null)
                {
                    value.Click += Delete_Click;
                }
                _Delete = value;
            }
        }

        public void UpdateEnable()
        {
            var s = _Handler.SelectedAvailable;
            var c = _Handler.NewItemAvailabel &&
                Clipboard.ContainsData(_Handler.DataID) &&
                _Handler.ClipboardDataAvailable(Clipboard.GetData(_Handler.DataID));
            if (New != null)
            {
                New.Enabled = _Handler.NewItemAvailabel;
            }
            if (Cut != null)
            {
                Cut.Enabled = s;
            }
            if (Copy != null)
            {
                Copy.Enabled = s;
            }
            if (Paste != null)
            {
                Paste.Enabled = c;
            }
            if (Delete != null)
            {
                Delete.Enabled = s;
            }
        }

        private void New_Click(object sender, EventArgs e)
        {
            _Handler.DoNew();
        }

        private void Cut_Click(object sender, EventArgs e)
        {
            _Handler.DoCut();
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            _Handler.DoCopy();
        }

        private void Paste_Click(object sender, EventArgs e)
        {
            _Handler.DoPaste();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            _Handler.DoDelete();
        }
    }
}
