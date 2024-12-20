
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    public class WatermarkConfig
    {
        public WatermarkType Type = WatermarkType.Text;
        public ContentAlignment Placement = ContentAlignment.BottomRight;
        public int Offset = 5;
        public DrawText Text = new DrawText { DrawTextShadow = false };
        public DrawImage Image = new DrawImage();

        public Bitmap Apply(Bitmap bmp)
        {
            Text.Placement = Image.Placement = Placement;
            Text.Offset = Image.Offset = new Point(Offset, Offset);

            switch (Type)
            {
                default:
                case WatermarkType.Text:
                    return Text.Apply(bmp);
                case WatermarkType.Image:
                    return Image.Apply(bmp);
            }
        }
    }
}