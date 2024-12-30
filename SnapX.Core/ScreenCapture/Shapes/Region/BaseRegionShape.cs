
// SPDX-License-Identifier: GPL-3.0-or-later


namespace SnapX.Core.ScreenCapture.Shapes.Region
{
    public abstract class BaseRegionShape : BaseShape
    {
        public override ShapeCategory ShapeCategory { get; } = ShapeCategory.Region;
    }
}
