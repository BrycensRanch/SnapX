
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using SnapX.ScreenCaptureLib.Properties;
using System.Drawing;

namespace SnapX.ScreenCaptureLib
{
    public class BlurEffectShape : BaseEffectShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.EffectBlur;

        public override string OverlayText => Resources.Blur + $" [{BlurRadius}]";

        public int BlurRadius { get; set; }

        public override void OnConfigLoad()
        {
            BlurRadius = AnnotationOptions.BlurRadius;
        }

        public override void OnConfigSave()
        {
            AnnotationOptions.BlurRadius = BlurRadius;
        }

        public override void ApplyEffect(Bitmap bmp)
        {
            ImageHelpers.BoxBlur(bmp, BlurRadius);
        }

        public override void OnDrawFinal(Graphics g, Bitmap bmp)
        {
            if (BlurRadius > 1)
            {
                base.OnDrawFinal(g, bmp);
            }
        }
    }
}