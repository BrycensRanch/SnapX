
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;

namespace SnapX.ImageEffectsLib
{
    internal class Saturation : ImageEffect
    {
        [DefaultValue(1f)]
        public float Value { get; set; }

        public Saturation()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            using (bmp)
            {
                return ColorMatrixManager.Saturation(Value).Apply(bmp);
            }
        }

        protected override string GetSummary()
        {
            return Value.ToString();
        }
    }
}