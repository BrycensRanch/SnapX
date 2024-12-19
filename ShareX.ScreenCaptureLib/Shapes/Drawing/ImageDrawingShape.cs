
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShareX.ScreenCaptureLib
{
    public class ImageDrawingShape : BaseDrawingShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.DrawingImage;

        public Image Image { get; protected set; }
        public ImageInterpolationMode ImageInterpolationMode { get; protected set; }

        public override BaseShape Duplicate()
        {
            Image imageTemp = Image;
            Image = null;
            ImageDrawingShape shape = (ImageDrawingShape)base.Duplicate();
            shape.Image = imageTemp.CloneSafe();
            Image = imageTemp;
            return shape;
        }

        public override void OnConfigLoad()
        {
            ImageInterpolationMode = AnnotationOptions.ImageInterpolationMode;
        }

        public override void OnConfigSave()
        {
            AnnotationOptions.ImageInterpolationMode = ImageInterpolationMode;
        }

        public void SetImage(Image img, bool centerImage)
        {
            Dispose();

            Image = img;

            if (Image != null)
            {
                PointF location;
                Size size = Image.Size;

                if (centerImage)
                {
                    location = new PointF(Rectangle.X - (size.Width / 2), Rectangle.Y - (size.Height / 2));
                }
                else
                {
                    location = Rectangle.Location;
                }

                Rectangle = new RectangleF(location, size);
            }
        }

        public override void OnDraw(Graphics g)
        {
            DrawImage(g);
        }

        protected void DrawImage(Graphics g)
        {
            if (Image != null)
            {
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.InterpolationMode = ImageHelpers.GetInterpolationMode(ImageInterpolationMode);

                g.DrawImage(Image, Rectangle);

                g.PixelOffsetMode = PixelOffsetMode.Default;
                g.InterpolationMode = InterpolationMode.Bilinear;
            }
        }

        public override void OnMoved()
        {
            /*if (Manager.Form.IsEditorMode)
            {
                Manager.AutoResizeCanvas();
            }*/
        }

        public override void Dispose()
        {
            if (Image != null)
            {
                Image.Dispose();
            }
        }
    }
}