
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Drawing;

namespace SnapX.Core.ScreenCapture.Shapes.Drawing
{
    public class SpeechBalloonDrawingShape : TextDrawingShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.DrawingSpeechBalloon;

        private PointF tailPosition;

        public PointF TailPosition
        {
            get
            {
                return tailPosition;
            }
            private set
            {
                tailPosition = value;
                TailNode.Position = tailPosition;
            }
        }

        public bool TailVisible => !Rectangle.Contains(TailPosition);

        internal ResizeNode TailNode => Manager.ResizeNodes[(int)NodePosition.Extra];

        protected const float TailWidthMultiplier = 0.3f;

        public override void OnCreated()
        {
            AutoSize(true);
            TailPosition = Rectangle.Location.Add(0, Rectangle.Height + 30);
            OnCreated(false);
        }

        protected override void UseLightResizeNodes()
        {
            ChangeNodeShape(NodeShape.Square);
            Manager.ResizeNodes[(int)NodePosition.Extra].Shape = NodeShape.Circle;
        }

        public override void OnNodeVisible()
        {
            base.OnNodeVisible();

            TailNode.Position = TailPosition;
            TailNode.Visible = true;
        }

        public override void OnNodeUpdate()
        {
            base.OnNodeUpdate();

            if (TailNode.IsDragging)
            {
                TailPosition = Manager.Form.ScaledClientMousePosition;
            }
        }

        public override void Move(float x, float y)
        {
            base.Move(x, y);

            TailPosition = TailPosition.Add(x, y);
        }

        public override void OnDraw(Graphics g)
        {
            if (Rectangle.Width > 10 && Rectangle.Height > 10)
            {
                DrawSpeechBalloon(g);
                DrawText(g);
            }
        }

        protected void DrawSpeechBalloon(Graphics g)
        {
            if (Shadow)
            {
                if (IsBorderVisible)
                {
                    DrawSpeechBalloon(g, ShadowColor, BorderSize, Color.Transparent, Rectangle.LocationOffset(ShadowOffset), TailPosition.Add(ShadowOffset));
                }
                else if (FillColor.A == 255)
                {
                    DrawSpeechBalloon(g, Color.Transparent, 0, ShadowColor, Rectangle.LocationOffset(ShadowOffset), TailPosition.Add(ShadowOffset));
                }
            }

            DrawSpeechBalloon(g, BorderColor, BorderSize, FillColor, Rectangle, TailPosition);
        }

        protected void DrawSpeechBalloon(Graphics g, Color borderColor, int borderSize, Color fillColor, RectangleF rect, PointF tailPosition)
        {
            GraphicsPath gpTail = null;

            if (TailVisible)
            {
                gpTail = CreateTailPath(rect, tailPosition);
            }

            if (fillColor.A > 0)
            {
                using (Brush brush = new SolidBrush(fillColor))
                {
                    g.FillRectangle(brush, rect);
                }
            }

            if (gpTail != null)
            {
                g.SmoothingMode = SmoothingMode.HighQuality;

                if (fillColor.A > 0)
                {
                    g.ExcludeClip(rect.Round());

                    using (Brush brush = new SolidBrush(fillColor))
                    {
                        g.FillPath(brush, gpTail);
                    }

                    g.ResetClip();
                }

                if (borderSize > 0 && borderColor.A > 0)
                {
                    g.ExcludeClip(rect.Offset(-1).Round());

                    using (Pen pen = new Pen(borderColor, borderSize))
                    {
                        g.DrawPath(pen, gpTail);
                    }

                    g.ResetClip();
                }

                g.SmoothingMode = SmoothingMode.None;
            }

            if (borderSize > 0 && borderColor.A > 0)
            {
                if (gpTail != null)
                {
                    using (Region region = new Region(gpTail))
                    {
                        g.ExcludeClip(region);
                    }
                }

                using (Pen pen = new Pen(borderColor, borderSize) { Alignment = PenAlignment.Inset })
                {
                    g.DrawRectangleProper(pen, rect.Offset(borderSize - 1));
                }

                g.ResetClip();
            }

            if (gpTail != null)
            {
                gpTail.Dispose();
            }
        }

        protected GraphicsPath CreateTailPath(RectangleF rect, PointF tailPosition)
        {
            GraphicsPath gpTail = new GraphicsPath();
            PointF center = rect.Center();
            float rectAverageSize = (rect.Width + rect.Height) / 2;
            float tailWidth = TailWidthMultiplier * rectAverageSize;
            tailWidth = Math.Min(Math.Min(tailWidth, rect.Width), rect.Height);
            float tailOrigin = tailWidth / 2;
            int tailLength = (int)MathHelpers.Distance(center, tailPosition);
            gpTail.AddLine(0, -tailOrigin, 0, tailOrigin);
            gpTail.AddLine(0, tailOrigin, tailLength, 0);
            gpTail.CloseFigure();
            using (Matrix matrix = new Matrix())
            {
                matrix.Translate(center.X, center.Y);
                float tailDegree = MathHelpers.LookAtDegree(center, tailPosition);
                matrix.Rotate(tailDegree);
                gpTail.Transform(matrix);
            }
            return gpTail;
        }
    }
}
