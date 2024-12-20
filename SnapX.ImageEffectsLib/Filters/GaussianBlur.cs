
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System;
using System.ComponentModel;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    [Description("Gaussian blur")]
    internal class GaussianBlur : ImageEffect
    {
        private int radius;

        [DefaultValue(15)]
        public int Radius
        {
            get
            {
                return radius;
            }
            set
            {
                radius = Math.Max(value, 1);
            }
        }

        public GaussianBlur()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            using (bmp)
            {
                return ImageHelpers.GaussianBlur(bmp, Radius);
            }
        }

        protected override string GetSummary()
        {
            return Radius.ToString();
        }
    }
}