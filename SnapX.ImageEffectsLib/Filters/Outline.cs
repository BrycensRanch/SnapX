
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using SixLabors.ImageSharp;
using SnapX.Core.Utils;
using SnapX.Core.Utils.Extensions;

namespace SnapX.ImageEffectsLib.Filters;

internal class Outline : ImageEffect
{
    private int size;

    [DefaultValue(1)]
    public int Size
    {
        get
        {
            return size;
        }
        set
        {
            size = value.Max(1);
        }
    }

    private int padding;

    [DefaultValue(0)]
    public int Padding
    {
        get
        {
            return padding;
        }
        set
        {
            padding = value.Max(0);
        }
    }

    [DefaultValue(typeof(Color), "Black")]
    public Color Color { get; set; }

    [DefaultValue(false)]
    public bool OutlineOnly { get; set; }

    public Outline()
    {
        this.ApplyDefaultPropertyValues();
    }

    public override Image Apply(Image img)
    {
        return ImageHelpers.Outline(img, Size, Color, Padding, OutlineOnly);
    }

    protected override string GetSummary()
    {
        return Size.ToString();
    }
}
