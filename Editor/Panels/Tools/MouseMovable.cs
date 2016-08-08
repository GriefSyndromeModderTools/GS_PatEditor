using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GS_PatEditor.Editor.Panels.Tools
{
    class MouseMovable
    {
        private MouseButtons _Button;

        private int _X, _Y;
        private int _DownMouseX, _DownMouseY, _LastX, _LastY;
        private bool _IsButtonDown;

        public event Action<int, int> OnMoved;
        public event Action<int, int> OnMovedDiff;
        public event Action OnMoveFinished;
        public event EventFilter FilterMouseDown;

        public void SetPosition(int x, int y)
        {
            _X = x;
            _Y = y;
        }

        public MouseMovable(Control ctrl, MouseButtons button, int x, int y)
        {
            _Button = button;

            ctrl.MouseDown += ctrl_MouseDown;
            ctrl.MouseMove += ctrl_MouseMove;
            ctrl.MouseUp += ctrl_MouseUp;
            //TODO this event handler is never removed
            //be careful of memory leakage

            _X = x;
            _Y = y;
            _LastX = x;
            _LastY = y;
        }

        public bool IsMoving { get { return _IsButtonDown; } }

        public bool Enabled { get { return CheckFilter(); } }

        private void Update()
        {
            if (OnMoved != null)
            {
                OnMoved(_X, _Y);
            }
            if (OnMovedDiff != null)
            {
                OnMovedDiff(_X - _LastX, _Y - _LastY);
            }
        }

        void ctrl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button.HasFlag(_Button) && _IsButtonDown)
            {
                _IsButtonDown = false;
                if (OnMoveFinished != null)
                {
                    OnMoveFinished();
                }
            }
        }

        void ctrl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button.HasFlag(_Button) && _IsButtonDown)
            {
                _X = _LastX + e.X - _DownMouseX;
                _Y = _LastY + e.Y - _DownMouseY;
                Update();
            }
        }

        private void ctrl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == _Button) //only handle single button
            {
                if (!CheckFilter())
                {
                    return;
                }

                _IsButtonDown = true;

                _LastX = _X;
                _LastY = _Y;
                _DownMouseX = e.X;
                _DownMouseY = e.Y;
            }
        }

        private bool CheckFilter()
        {
            if (FilterMouseDown == null)
            {
                return true;
            }

            bool ret = true;
            FilterMouseDown(ref ret);

            return ret;
        }
    }
}
