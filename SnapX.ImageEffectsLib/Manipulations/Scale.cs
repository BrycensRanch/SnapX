
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System;
using System.ComponentModel;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    internal class Scale : ImageEffect
    {
        [DefaultValue(100f), Description("Use width percentage as 0 to maintain aspect ratio by automatically adjusting width.")]
        public float WidthPercentage { get; set; }

        [DefaultValue(0f), Description("Use height percentage as 0 to maintain aspect ratio by automatically adjusting height.")]
        public float HeightPercentage { get; set; }

        public Scale()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            if (WidthPercentage <= 0 && HeightPercentage <= 0)
            {
                return bmp;
            }

            int width = (int)Math.Round(WidthPercentage / 100 * bmp.Width);
            int height = (int)Math.Round(HeightPercentage / 100 * bmp.Height);
            Size size = ImageHelpers.ApplyAspectRatio(width, height, bmp);

            return ImageHelpers.ResizeImage(bmp, size);
        }

        protected override string GetSummary()
        {
            string summary = WidthPercentage.ToString();

            if (WidthPercentage > 0)
            {
                summary += "%";
            }

            summary += ", " + HeightPercentage.ToString();

            if (HeightPercentage > 0)
            {
                summary += "%";
            }

            return summary;
        }
    }
}