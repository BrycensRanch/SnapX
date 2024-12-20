
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using ShareX.ScreenCaptureLib.Properties;
using System.Drawing;

namespace ShareX.ScreenCaptureLib
{
    public class PixelateEffectShape : BaseEffectShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.EffectPixelate;

        public override string OverlayText => Resources.Pixelate + $" [{PixelSize}]";

        public int PixelSize { get; set; }

        public override void OnConfigLoad()
        {
            PixelSize = AnnotationOptions.PixelateSize;
        }

        public override void OnConfigSave()
        {
            AnnotationOptions.PixelateSize = PixelSize;
        }

        public override void ApplyEffect(Bitmap bmp)
        {
            ImageHelpers.Pixelate(bmp, PixelSize);
        }

        public override void OnDrawFinal(Graphics g, Bitmap bmp)
        {
            if (PixelSize > 1)
            {
                base.OnDrawFinal(g, bmp);
            }
        }
    }
}