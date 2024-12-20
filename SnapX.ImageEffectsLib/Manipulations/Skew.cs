
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    internal class Skew : ImageEffect
    {
        [DefaultValue(0), Description("How much pixel skew left to right.")]
        public int Horizontally { get; set; }

        [DefaultValue(0), Description("How much pixel skew top to bottom.")]
        public int Vertically { get; set; }

        public Skew()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            if (Horizontally == 0 && Vertically == 0)
            {
                return bmp;
            }

            return ImageHelpers.AddSkew(bmp, Horizontally, Vertically);
        }

        protected override string GetSummary()
        {
            return $"{Horizontally}px, {Vertically}px";
        }
    }
}