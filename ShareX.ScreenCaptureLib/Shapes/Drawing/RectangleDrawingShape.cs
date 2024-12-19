
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShareX.ScreenCaptureLib
{
    public class RectangleDrawingShape : BaseDrawingShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.DrawingRectangle;

        public int CornerRadius { get; set; }

        public override void OnConfigLoad()
        {
            base.OnConfigLoad();
            CornerRadius = AnnotationOptions.DrawingCornerRadius;
        }

        public override void OnConfigSave()
        {
            base.OnConfigSave();
            AnnotationOptions.DrawingCornerRadius = CornerRadius;
        }

        public override void OnDraw(Graphics g)
        {
            DrawRectangle(g);
        }

        protected void DrawRectangle(Graphics g)
        {
            if (Shadow)
            {
                if (IsBorderVisible)
                {
                    DrawRectangle(g, ShadowColor, BorderSize, BorderStyle, Color.Transparent, Rectangle.LocationOffset(ShadowOffset), CornerRadius);
                }
                else if (FillColor.A == 255)
                {
                    DrawRectangle(g, Color.Transparent, 0, BorderStyle, ShadowColor, Rectangle.LocationOffset(ShadowOffset), CornerRadius);
                }
            }

            DrawRectangle(g, BorderColor, BorderSize, BorderStyle, FillColor, Rectangle, CornerRadius);
        }

        protected void DrawRectangle(Graphics g, Color borderColor, int borderSize, BorderStyle borderStyle, Color fillColor, RectangleF rect, int cornerRadius)
        {
            Brush brush = null;
            Pen pen = null;

            try
            {
                if (fillColor.A > 0)
                {
                    brush = new SolidBrush(fillColor);
                }

                if (borderSize > 0 && borderColor.A > 0)
                {
                    pen = new Pen(borderColor, borderSize);
                    pen.DashStyle = (DashStyle)borderStyle;
                }

                if (cornerRadius > 0)
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;

                    if (borderSize.IsEvenNumber())
                    {
                        g.PixelOffsetMode = PixelOffsetMode.Half;
                    }
                }

                g.DrawRoundedRectangle(brush, pen, rect, cornerRadius);

                g.SmoothingMode = SmoothingMode.None;
                g.PixelOffsetMode = PixelOffsetMode.Default;
            }
            finally
            {
                if (brush != null) brush.Dispose();
                if (pen != null) pen.Dispose();
            }
        }

        public override void OnShapePathRequested(GraphicsPath gp, RectangleF rect)
        {
            gp.AddRoundedRectangle(rect, CornerRadius);
        }
    }
}