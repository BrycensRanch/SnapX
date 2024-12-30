
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Drawing;

namespace SnapX.ScreenCaptureLib.Helpers
{
    public class InputManager
    {
        public Point MousePosition => mouseState.Position;

        public Point PreviousMousePosition => oldMouseState.Position;

        public Point ClientMousePosition => mouseState.ClientPosition;

        public Point PreviousClientMousePosition => oldMouseState.ClientPosition;

        public Point MouseVelocity => new Point(ClientMousePosition.X - PreviousClientMousePosition.X, ClientMousePosition.Y - PreviousClientMousePosition.Y);

        public bool IsMouseMoved => MouseVelocity.X != 0 || MouseVelocity.Y != 0;

        // private MouseState mouseState = new MouseState();
        // private MouseState oldMouseState;

        // public void Update(Control control)
        // {
        //     oldMouseState = mouseState;
        //     mouseState.Update(control);
        // }
        //
        // public bool IsMouseDown(MouseButtons button)
        // {
        //     return mouseState.Buttons.HasFlag(button);
        // }
        //
        // public bool IsBeforeMouseDown(MouseButtons button)
        // {
        //     return oldMouseState.Buttons.HasFlag(button);
        // }
        //
        // public bool IsMousePressed(MouseButtons button)
        // {
        //     return IsMouseDown(button) && !IsBeforeMouseDown(button);
        // }
        //
        // public bool IsMouseReleased(MouseButtons button)
        // {
        //     return !IsMouseDown(button) && IsBeforeMouseDown(button);
        // }
    }
}
