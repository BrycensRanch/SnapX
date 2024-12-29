
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;

namespace SnapX.ImageEffectsLib
{
    internal class Grayscale : ImageEffect
    {
        [DefaultValue(1f)]
        public float Value { get; set; }

        public Grayscale()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            using (bmp)
            {
                return ColorMatrixManager.Grayscale(Value).Apply(bmp);
            }
        }

        protected override string GetSummary()
        {
            return Value.ToString();
        }
    }
}