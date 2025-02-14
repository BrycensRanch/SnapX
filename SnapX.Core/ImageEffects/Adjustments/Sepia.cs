
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.ImageEffects.Adjustments;

internal class Sepia : ImageEffect
{
    [DefaultValue(1f)]
    public float Value { get; set; }

    public Sepia()
    {
        this.ApplyDefaultPropertyValues();
    }

    public override Image Apply(Image img)
    {
        img.Mutate(ctx => ctx.Sepia(Value));
        return img;
    }

    protected override string GetSummary()
    {
        return Value.ToString();
    }
}
