
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShareX.ScreenCaptureLib
{
    public class TextDrawingOptions
    {
        public string Font { get; set; } = AnnotationOptions.DefaultFont;
        public int Size { get; set; } = 18;
        public Color Color { get; set; } = Color.White;
        public bool Bold { get; set; } = false;
        public bool Italic { get; set; } = false;
        public bool Underline { get; set; } = false;
        public StringAlignment AlignmentHorizontal { get; set; } = StringAlignment.Center;
        public StringAlignment AlignmentVertical { get; set; } = StringAlignment.Center;

        public FontStyle Style
        {
            get
            {
                FontStyle style = FontStyle.Regular;

                if (Bold)
                {
                    style |= FontStyle.Bold;
                }

                if (Italic)
                {
                    style |= FontStyle.Italic;
                }

                if (Underline)
                {
                    style |= FontStyle.Underline;
                }

                return style;
            }
        }

        public bool Gradient { get; set; } = false;
        public Color Color2 { get; set; } = Color.FromArgb(240, 240, 240);
        public LinearGradientMode GradientMode { get; set; } = LinearGradientMode.Vertical;
        public bool EnterKeyNewLine { get; set; } = false;
    }
}