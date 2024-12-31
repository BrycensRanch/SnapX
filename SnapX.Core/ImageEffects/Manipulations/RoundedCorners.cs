
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using SixLabors.ImageSharp;
using SnapX.Core.Utils;
using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.ImageEffects.Manipulations
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

        public override Image Apply(Image img)
        {
            return ImageHelpers.RoundedCorners(img, CornerRadius);
        }

        protected override string GetSummary()
        {
            return CornerRadius.ToString();
        }
    }
}
