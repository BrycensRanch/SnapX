// SPDX-License-Identifier: GPL-3.0-or-later

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace SnapX.Core.ImageEffects.Filters;

internal class Smooth : ImageEffect
{
    public override Image Apply(Image img)
    {
        // Listen... I'm trying my best.
        img.Mutate(ctx => ctx.GaussianBlur(5));

        return img;
    }
}
