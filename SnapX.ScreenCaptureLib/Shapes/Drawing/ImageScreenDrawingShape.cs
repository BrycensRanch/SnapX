
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Drawing;

namespace ShareX.ScreenCaptureLib
{
    public class ImageScreenDrawingShape : ImageDrawingShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.DrawingImageScreen;

        public override void OnCreated()
        {
            if (IsValidShape)
            {
                Rectangle = RectangleInsideCanvas;
                Image = Manager.CropImage(Rectangle);
            }

            if (Image == null)
            {
                Remove();
            }
            else
            {
                base.OnCreated();
            }
        }

        public override void OnDraw(Graphics g)
        {
            if (Image == null)
            {
                if (IsValidShape)
                {
                    Manager.DrawRegionArea(g, RectangleInsideCanvas, true);
                }
            }
            else
            {
                base.OnDraw(g);
            }
        }
    }
}