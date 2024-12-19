
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    internal class Blur : ImageEffect
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
                radius = value.Max(3);

                if (radius.IsEvenNumber())
                {
                    radius++;
                }
            }
        }

        public Blur()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            ImageHelpers.BoxBlur(bmp, Radius);
            return bmp;
        }

        protected override string GetSummary()
        {
            return Radius.ToString();
        }
    }
}