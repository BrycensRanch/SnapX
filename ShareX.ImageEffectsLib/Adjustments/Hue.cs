
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    internal class Hue : ImageEffect
    {
        [DefaultValue(0f), Description("From 0 to 360")]
        public float Angle { get; set; }

        public Hue()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            using (bmp)
            {
                return ColorMatrixManager.Hue(Angle).Apply(bmp);
            }
        }

        protected override string GetSummary()
        {
            return Angle + "°";
        }
    }
}