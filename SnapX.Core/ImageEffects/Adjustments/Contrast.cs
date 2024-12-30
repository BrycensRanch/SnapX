
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.ImageEffects.Adjustments;

internal class Contrast : ImageEffect
{
    [DefaultValue(1f), Description("Pixel color = Pixel color * Value\r\nExample 1.5 will increase color of pixel 50%")]
    public float Value { get; set; }

    public Contrast()
    {
        this.ApplyDefaultPropertyValues();
    }

    public override Image Apply(Image img)
    {
        img.Mutate(ctx => ctx.Contrast(Value));
        return img;
    }

    protected override string GetSummary()
    {
        return Value.ToString();
    }
}
