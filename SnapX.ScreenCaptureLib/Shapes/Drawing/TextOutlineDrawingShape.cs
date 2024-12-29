
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SnapX.ScreenCaptureLib
{
    public class TextOutlineDrawingShape : TextDrawingShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.DrawingTextOutline;

        public override bool SupportGradient { get; } = true;

        public override void OnConfigLoad()
        {
            TextOptions = AnnotationOptions.TextOutlineOptions.Copy();
            BorderColor = AnnotationOptions.TextOutlineBorderColor;
            BorderSize = AnnotationOptions.TextOutlineBorderSize;
            Shadow = AnnotationOptions.Shadow;
            ShadowColor = AnnotationOptions.ShadowColor;
            ShadowOffset = AnnotationOptions.ShadowOffset;
        }

        public override void OnConfigSave()
        {
            AnnotationOptions.TextOutlineOptions = TextOptions;
            AnnotationOptions.TextOutlineBorderColor = BorderColor;
            AnnotationOptions.TextOutlineBorderSize = BorderSize;
            AnnotationOptions.Shadow = Shadow;
            AnnotationOptions.ShadowColor = ShadowColor;
            AnnotationOptions.ShadowOffset = ShadowOffset;
        }

        public override void OnDraw(Graphics g)
        {
            DrawTextWithOutline(g, Text, TextOptions, TextOptions.Color, BorderColor, BorderSize, Rectangle);
        }

        protected void DrawTextWithOutline(Graphics g, string text, TextDrawingOptions options, Color textColor, Color borderColor, int borderSize, RectangleF rect)
        {
            if (!string.IsNullOrEmpty(text) && rect.Width > 10 && rect.Height > 10)
            {
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.FillMode = FillMode.Winding;

                    using (Font font = new Font(options.Font, options.Size, options.Style))
                    using (StringFormat sf = new StringFormat { Alignment = options.AlignmentHorizontal, LineAlignment = options.AlignmentVertical })
                    {
                        float emSize = g.DpiY * font.SizeInPoints / 72;
                        gp.AddString(text, font.FontFamily, (int)font.Style, emSize, rect, sf);
                    }

                    RectangleF pathRect = gp.GetBounds();

                    if (pathRect.IsEmpty) return;

                    g.SmoothingMode = SmoothingMode.HighQuality;

                    if (Shadow)
                    {
                        using (Matrix matrix = new Matrix())
                        {
                            matrix.Translate(ShadowOffset.X, ShadowOffset.Y);
                            gp.Transform(matrix);

                            if (IsBorderVisible)
                            {
                                using (Pen shadowPen = new Pen(ShadowColor, borderSize) { LineJoin = LineJoin.Round })
                                {
                                    g.DrawPath(shadowPen, gp);
                                }
                            }
                            else
                            {
                                using (Brush shadowBrush = new SolidBrush(ShadowColor))
                                {
                                    g.FillPath(shadowBrush, gp);
                                }
                            }

                            matrix.Reset();
                            matrix.Translate(-ShadowOffset.X, -ShadowOffset.Y);
                            gp.Transform(matrix);
                        }
                    }

                    if (IsBorderVisible)
                    {
                        using (Pen borderPen = new Pen(borderColor, borderSize) { LineJoin = LineJoin.Round })
                        {
                            g.DrawPath(borderPen, gp);
                        }
                    }

                    Brush textBrush = null;

                    try
                    {
                        if (TextOptions.Gradient)
                        {
                            textBrush = new LinearGradientBrush(pathRect.Round().Offset(1), textColor, TextOptions.Color2, TextOptions.GradientMode);
                        }
                        else
                        {
                            textBrush = new SolidBrush(textColor);
                        }

                        g.FillPath(textBrush, gp);
                    }
                    finally
                    {
                        if (textBrush != null)
                        {
                            textBrush.Dispose();
                        }
                    }

                    g.SmoothingMode = SmoothingMode.None;
                }
            }
        }
    }
}