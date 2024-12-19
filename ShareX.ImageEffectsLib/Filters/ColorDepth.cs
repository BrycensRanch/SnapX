
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;

namespace ShareX.ImageEffectsLib
{
    [Description("Color depth")]
    internal class ColorDepth : ImageEffect
    {
        private int bitsPerChannel;

        [DefaultValue(4)]
        public int BitsPerChannel
        {
            get
            {
                return bitsPerChannel;
            }
            set
            {
                bitsPerChannel = MathHelpers.Clamp(value, 1, 8);
            }
        }

        public ColorDepth()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            ImageHelpers.ColorDepth(bmp, BitsPerChannel);
            return bmp;
        }

        protected override string GetSummary()
        {
            string summary = BitsPerChannel + " bit";

            if (BitsPerChannel > 1)
            {
                summary += "s";
            }

            return summary;
        }
    }
}