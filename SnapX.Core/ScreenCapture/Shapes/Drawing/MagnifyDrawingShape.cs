
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Drawing;

namespace SnapX.Core.ScreenCapture.Shapes.Drawing
{
    public class MagnifyDrawingShape : EllipseDrawingShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.DrawingMagnify;

        public int MagnifyStrength { get; set; } = 200;
        public ImageInterpolationMode ImageInterpolationMode { get; set; }

        public MagnifyDrawingShape()
        {
            ForceProportionalResizing = true;
        }

        public override void OnConfigLoad()
        {
            base.OnConfigLoad();
            MagnifyStrength = AnnotationOptions.MagnifyStrength;
            ImageInterpolationMode = AnnotationOptions.ImageInterpolationMode;
        }

        public override void OnConfigSave()
        {
            base.OnConfigSave();
            AnnotationOptions.MagnifyStrength = MagnifyStrength;
            AnnotationOptions.ImageInterpolationMode = ImageInterpolationMode;
        }

        public override void OnDraw(Graphics g)
        {
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.InterpolationMode = ImageHelpers.GetInterpolationMode(ImageInterpolationMode);

            using (GraphicsPath gp = new GraphicsPath())
            {
                gp.AddEllipse(Rectangle);
                g.SetClip(gp);

                float magnify = Math.Max(MagnifyStrength, 100) / 100f;
                int newWidth = (int)(Rectangle.Width / magnify);
                int newHeight = (int)(Rectangle.Height / magnify);

                g.DrawImage(Manager.Form.Canvas, Rectangle,
                    new RectangleF(Rectangle.X + (Rectangle.Width / 2) - (newWidth / 2) - Manager.Form.CanvasRectangle.X + Manager.RenderOffset.X,
                    Rectangle.Y + (Rectangle.Height / 2) - (newHeight / 2) - Manager.Form.CanvasRectangle.Y + Manager.RenderOffset.Y,
                    newWidth, newHeight), GraphicsUnit.Pixel);

                g.ResetClip();
            }

            g.PixelOffsetMode = PixelOffsetMode.Default;
            g.InterpolationMode = InterpolationMode.Bilinear;

            DrawEllipse(g);
        }
    }
}
