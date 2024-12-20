
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    [Description("Rounded corners")]
    internal class RoundedCorners : ImageEffect
    {
        private int cornerRadius;

        [DefaultValue(20)]
        public int CornerRadius
        {
            get
            {
                return cornerRadius;
            }
            set
            {
                cornerRadius = value.Max(0);
            }
        }

        public RoundedCorners()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            return ImageHelpers.RoundedCorners(bmp, CornerRadius);
        }

        protected override string GetSummary()
        {
            return CornerRadius.ToString();
        }
    }
}