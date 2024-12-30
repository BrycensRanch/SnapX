
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.ImageEffects.Adjustments;

internal class Hue : ImageEffect
{
    [DefaultValue(0f), Description("From 0 to 360")]
    public float Angle { get; set; }

    public Hue()
    {
        this.ApplyDefaultPropertyValues();
    }

    public override Image Apply(Image img)
    {
        img.Mutate(ctx => ctx.Hue(Angle));
        return img;
    }

    protected override string GetSummary()
    {
        return Angle + "°";
    }
}
