
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SnapX.Core.Utils;
using SnapX.Core.Utils.Extensions;

namespace SnapX.ImageEffectsLib.Adjustments;

[Description("Selective color")]
internal class SelectiveColor : ImageEffect
{
    [DefaultValue(typeof(Color), "White")]
    public Color LightColor { get; set; }

    [DefaultValue(typeof(Color), "Black")]
    public Color DarkColor { get; set; }

    [DefaultValue(10)]
    public int PaletteSize { get; set; }

    public SelectiveColor()
    {
        this.ApplyDefaultPropertyValues();
    }

    public override Image Apply(Image img)
    {
        img.Mutate(ctx => ctx.SelectiveColor(LightColor, DarkColor, MathHelpers.Clamp(PaletteSize, 2, 100)));
        return img;
    }
}
