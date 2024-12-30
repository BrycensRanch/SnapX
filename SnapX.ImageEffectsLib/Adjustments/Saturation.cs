
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SnapX.Core.Utils.Extensions;

namespace SnapX.ImageEffectsLib.Adjustments;

internal class Saturation : ImageEffect
{
    [DefaultValue(1f)]
    public float Value { get; set; }

    public Saturation()
    {
        this.ApplyDefaultPropertyValues();
    }

    public override Image Apply(Image img)
    {
        img.Mutate(ctx => ctx.Saturate(Value));
        return img;
    }

    protected override string GetSummary()
    {
        return Value.ToString();
    }
}
