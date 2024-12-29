
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;

namespace SnapX.ImageEffectsLib
{
    internal class Gamma : ImageEffect
    {
        [DefaultValue(1f), Description("Min 0.1, Max 5.0")]
        public float Value { get; set; }

        public Gamma()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            using (bmp)
            {
                return ColorMatrixManager.ChangeGamma(bmp, Value);
            }
        }

        protected override string GetSummary()
        {
            return Value.ToString();
        }
    }
}