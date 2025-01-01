
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using SixLabors.ImageSharp;
using SnapX.Core.Utils;
using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.ImageEffects.Manipulations;

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

    public override Image Apply(Image img)
    {
        return ImageHelpers.AutoCropImage(img, true, Sides, Padding);
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
