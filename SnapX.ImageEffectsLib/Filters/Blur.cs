
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using SixLabors.ImageSharp;
using SnapX.Core.Utils;
using SnapX.Core.Utils.Extensions;

namespace SnapX.ImageEffectsLib.Filters
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

        public override Image Apply(Image img)
        {
            ImageHelpers.BoxBlur(img, Radius);
            return img;
        }

        protected override string GetSummary()
        {
            return Radius.ToString();
        }
    }
}
