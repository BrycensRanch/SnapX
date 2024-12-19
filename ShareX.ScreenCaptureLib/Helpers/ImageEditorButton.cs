
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShareX.ScreenCaptureLib
{
    internal class ImageEditorButton : ImageEditorControl
    {
        public string Text { get; set; }
        public Color ButtonColor { get; set; }
        public int ButtonDepth { get; set; } = 3;
        public Color ButtonDepthColor => ColorHelpers.DarkerColor(ButtonColor, 0.5f);

        public override void OnDraw(Graphics g)
        {
            RectangleF rect = Rectangle;

            if (IsCursorHover)
            {
                rect = rect.LocationOffset(0, ButtonDepth);
            }

            g.SmoothingMode = SmoothingMode.HighQuality;

            using (SolidBrush buttonBrush = new SolidBrush(ButtonColor))
            {
                g.PixelOffsetMode = PixelOffsetMode.Half;

                if (!IsCursorHover)
                {
                    using (SolidBrush buttonDepthBrush = new SolidBrush(ButtonDepthColor))
                    {
                        g.DrawRoundedRectangle(buttonDepthBrush, rect.LocationOffset(0, ButtonDepth), 5);
                    }
                }

                g.DrawRoundedRectangle(buttonBrush, rect, 5);

                g.PixelOffsetMode = PixelOffsetMode.Default;
            }

            g.SmoothingMode = SmoothingMode.None;

            using (Font font = new Font("Arial", 18))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (SolidBrush textDepthBrush = new SolidBrush(ButtonDepthColor))
            {
                g.DrawString(Text, font, textDepthBrush, rect.LocationOffset(0, 4), sf);
                g.DrawString(Text, font, Brushes.White, rect.LocationOffset(0, 2), sf);
            }
        }
    }
}