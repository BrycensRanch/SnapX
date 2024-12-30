
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.ImageEffects.Adjustments;
internal class Gamma : ImageEffect
{
    [DefaultValue(1f), Description("Min 0.1, Max 5.0")]
    public float Value { get; set; }

    public Gamma()
    {
        this.ApplyDefaultPropertyValues();
    }

    public override Image Apply(Image img)
    {
        img.Mutate(ctx => ctx.ApplyGamma(Value));

        return img;
    }

    protected override string GetSummary()
    {
        return Value.ToString();
    }
}
