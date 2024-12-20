
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ShareX.ImageEffectsLib
{
    [Description("Auto crop")]
    internal class AutoCrop : ImageEffect
    {
        [DefaultValue(AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)]
        public AnchorStyles Sides { get; set; }

        [DefaultValue(0)]
        public int Padding { get; set; }

        public AutoCrop()
        {
            this.ApplyDefaultPropertyValues();
        }

        public override Bitmap Apply(Bitmap bmp)
        {
            return ImageHelpers.AutoCropImage(bmp, true, Sides, Padding);
        }

        protected override string GetSummary()
        {
            if (Padding > 0)
            {
                return Padding.ToString();
            }

            return null;
        }
    }
}