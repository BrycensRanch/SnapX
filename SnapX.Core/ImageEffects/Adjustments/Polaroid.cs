// SPDX-License-Identifier: GPL-3.0-or-later

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace SnapX.Core.ImageEffects.Adjustments;

internal class Polaroid : ImageEffect
{
    public override Image Apply(Image img)
    {
        img.Mutate(ctx => ctx.Polaroid());
        return img;
    }
}
