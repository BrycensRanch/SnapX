
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.ImageEffects.Adjustments;

internal class Brightness : ImageEffect
{
    [DefaultValue(0f), Description("Pixel color = Pixel color + Value\r\nExample 0.5 will increase color of pixel 127.5")]
    public float Value { get; set; }

    public Brightness()
    {
        this.ApplyDefaultPropertyValues();
    }

    public override Image Apply(Image img)
    {
        img.Mutate(ctx =>
        {
            ctx.Brightness(Value);
        });
        return img;
    }

    protected override string GetSummary()
    {
        return Value.ToString();
    }
}
