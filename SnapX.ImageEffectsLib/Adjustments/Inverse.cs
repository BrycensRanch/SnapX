// SPDX-License-Identifier: GPL-3.0-or-later

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace SnapX.ImageEffectsLib.Adjustments;

internal class Inverse : ImageEffect
{
    public override Image Apply(Image img)
    {
        img.Mutate(ctx => ctx.Invert());
        return img;
    }
}
