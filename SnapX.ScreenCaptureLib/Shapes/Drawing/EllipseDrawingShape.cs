
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SnapX.ScreenCaptureLib
{
    public class EllipseDrawingShape : BaseDrawingShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.DrawingEllipse;

        public override void OnDraw(Graphics g)
        {
            DrawEllipse(g);
        }

        protected void DrawEllipse(Graphics g)
        {
            if (Shadow)
            {
                if (IsBorderVisible)
                {
                    DrawEllipse(g, ShadowColor, BorderSize, BorderStyle, Color.Transparent, Rectangle.LocationOffset(ShadowOffset));
                }
                else if (FillColor.A == 255)
                {
                    DrawEllipse(g, Color.Transparent, 0, BorderStyle, ShadowColor, Rectangle.LocationOffset(ShadowOffset));
                }
            }

            DrawEllipse(g, BorderColor, BorderSize, BorderStyle, FillColor, Rectangle);
        }

        protected void DrawEllipse(Graphics g, Color borderColor, int borderSize, BorderStyle borderStyle, Color fillColor, RectangleF rect)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;

            if (fillColor.A > 0)
            {
                using (Brush brush = new SolidBrush(fillColor))
                {
                    g.FillEllipse(brush, rect);
                }
            }

            if (borderSize > 0 && borderColor.A > 0)
            {
                using (Pen pen = new Pen(borderColor, borderSize))
                {
                    pen.DashStyle = (DashStyle)borderStyle;

                    g.DrawEllipse(pen, rect);
                }
            }

            g.SmoothingMode = SmoothingMode.None;
        }

        public override void OnShapePathRequested(GraphicsPath gp, RectangleF rect)
        {
            gp.AddEllipse(rect);
        }
    }
}