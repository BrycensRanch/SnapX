
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Drawing;

namespace SnapX.Core.ScreenCapture.Shapes.Drawing
{
    public class CursorDrawingShape : ImageDrawingShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.DrawingCursor;

        public void UpdateCursor(IntPtr cursorHandle, Point position)
        {
            Icon icon = Icon.FromHandle(cursorHandle);
            Bitmap bmpCursor = icon.ToBitmap();
            UpdateCursor(bmpCursor, position);
        }

        public void UpdateCursor(Bitmap bmpCursor, Point position)
        {
            Dispose();
            Image = bmpCursor;
            Rectangle = new Rectangle(position, Image.Size);
        }

        public override void ShowNodes()
        {
        }

        public override void OnCreating()
        {
            Manager.IsMoving = true;
            UpdateCursor(Manager.GetSelectedCursor().Handle, Manager.Form.ScaledClientMousePosition.Round());
            OnCreated();
        }

        public override void OnDraw(Graphics g)
        {
            if (Image != null)
            {
                g.DrawImage(Image, Rectangle);

                if (!Manager.IsRenderingOutput && Manager.CurrentTool == ShapeType.DrawingCursor)
                {
                    Manager.DrawRegionArea(g, Rectangle.Round(), false);
                }
            }
        }

        public override void Resize(int x, int y, bool fromBottomRight)
        {
            Move(x, y);
        }
    }
}
