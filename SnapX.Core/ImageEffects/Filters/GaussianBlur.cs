
// SPDX-License-Identifier: GPL-3.0-or-later

using System.ComponentModel;
using SixLabors.ImageSharp;
using SnapX.Core.Utils;
using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.ImageEffects.Filters;
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

    public override Image Apply(Image img)
    {
        using (img)
        {
            return ImageHelpers.GaussianBlur(img, Radius);
        }
    }

    protected override string GetSummary()
    {
        return Radius.ToString();
    }
}
