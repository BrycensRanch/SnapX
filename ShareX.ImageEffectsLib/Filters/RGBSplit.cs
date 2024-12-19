
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;

namespace ShareX.ImageEffectsLib
{
    [Description("RGB split")]
    internal class RGBSplit : ImageEffect
    {
        [DefaultValue(typeof(Point), "-5, 0")]
        public Point OffsetRed { get; set; } = new Point(-5, 0);

        [DefaultValue(typeof(Point), "0, 0")]
        public Point OffsetGreen { get; set; }

        [DefaultValue(typeof(Point), "5, 0")]
        public Point OffsetBlue { get; set; } = new Point(5, 0);

        public override Bitmap Apply(Bitmap bmp)
        {
            Bitmap bmpResult = bmp.CreateEmptyBitmap();

            using (UnsafeBitmap source = new UnsafeBitmap(bmp, true, ImageLockMode.ReadOnly))
            using (UnsafeBitmap dest = new UnsafeBitmap(bmpResult, true, ImageLockMode.WriteOnly))
            {
                int right = source.Width - 1;
                int bottom = source.Height - 1;

                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        ColorBgra colorR = source.GetPixel(MathHelpers.Clamp(x - OffsetRed.X, 0, right), MathHelpers.Clamp(y - OffsetRed.Y, 0, bottom));
                        ColorBgra colorG = source.GetPixel(MathHelpers.Clamp(x - OffsetGreen.X, 0, right), MathHelpers.Clamp(y - OffsetGreen.Y, 0, bottom));
                        ColorBgra colorB = source.GetPixel(MathHelpers.Clamp(x - OffsetBlue.X, 0, right), MathHelpers.Clamp(y - OffsetBlue.Y, 0, bottom));
                        ColorBgra shiftedColor = new ColorBgra((byte)(colorB.Blue * colorB.Alpha / 255), (byte)(colorG.Green * colorG.Alpha / 255),
                            (byte)(colorR.Red * colorR.Alpha / 255), (byte)((colorR.Alpha + colorG.Alpha + colorB.Alpha) / 3));
                        dest.SetPixel(x, y, shiftedColor);
                    }
                }
            }

            return bmpResult;
        }
    }
}