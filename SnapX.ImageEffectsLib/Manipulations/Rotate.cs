
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    internal class Rotate : ImageEffect
    {
        [DefaultValue(0f), Description("Choose a value between -360 and 360.")]
        public float Angle { get; set; }

        [DefaultValue(true), Description("If true, output image will be larger than the input and no clipping will occur.")]
        public bool Upsize { get; set; }

        [DefaultValue(false), Description("Upsize must be false for this setting to work. If true, clipping will occur or else image size will be reduced.")]
        public bool Clip { get; set; }

        public Rotate()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            if (Angle == 0)
            {
                return bmp;
            }

            using (bmp)
            {
                return ImageHelpers.RotateImage(bmp, Angle, Upsize, Clip);
            }
        }

        protected override string GetSummary()
        {
            return Angle + "°";
        }
    }
}