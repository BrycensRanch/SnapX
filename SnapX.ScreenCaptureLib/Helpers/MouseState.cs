
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Drawing;
using System.Windows.Forms;

namespace SnapX.ScreenCaptureLib
{
    public struct MouseState
    {
        public MouseButtons Buttons { get; private set; }
        public Point Position { get; private set; }
        public Point ClientPosition { get; private set; }

        public void Update(Control control)
        {
            Buttons = Control.MouseButtons;
            Position = Control.MousePosition;

            if (control != null)
            {
                ClientPosition = control.PointToClient(Position);
            }
            else
            {
                ClientPosition = CaptureHelpers.ScreenToClient(Position);
            }
        }
    }
}