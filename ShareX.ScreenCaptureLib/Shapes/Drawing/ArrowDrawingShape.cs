
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShareX.ScreenCaptureLib
{
    public class ArrowDrawingShape : LineDrawingShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.DrawingArrow;

        public ArrowHeadDirection ArrowHeadDirection { get; set; }

        public override void OnConfigLoad()
        {
            base.OnConfigLoad();
            ArrowHeadDirection = AnnotationOptions.ArrowHeadDirection;
        }

        public override void OnConfigSave()
        {
            base.OnConfigSave();
            AnnotationOptions.ArrowHeadDirection = ArrowHeadDirection;
        }

        protected override Pen CreatePen(Color borderColor, int borderSize, BorderStyle borderStyle)
        {
            using (GraphicsPath gp = new GraphicsPath())
            {
                int arrowWidth = 2, arrowHeight = 6, arrowCurve = 1;
                gp.AddLine(new Point(0, 0), new Point(-arrowWidth, -arrowHeight));
                gp.AddCurve(new Point[] { new Point(-arrowWidth, -arrowHeight), new Point(0, -arrowHeight + arrowCurve), new Point(arrowWidth, -arrowHeight) });
                gp.CloseFigure();

                CustomLineCap lineCap = new CustomLineCap(gp, null)
                {
                    BaseInset = arrowHeight - arrowCurve
                };

                Pen pen = new Pen(borderColor, borderSize);

                if (ArrowHeadDirection == ArrowHeadDirection.Both && MathHelpers.Distance(Points[0], Points[Points.Length - 1]) > arrowHeight * borderSize * 2)
                {
                    pen.CustomEndCap = pen.CustomStartCap = lineCap;
                }
                else if (ArrowHeadDirection == ArrowHeadDirection.Start)
                {
                    pen.CustomStartCap = lineCap;
                }
                else
                {
                    pen.CustomEndCap = lineCap;
                }

                pen.LineJoin = LineJoin.Round;
                pen.DashStyle = (DashStyle)borderStyle;
                return pen;
            }
        }
    }
}