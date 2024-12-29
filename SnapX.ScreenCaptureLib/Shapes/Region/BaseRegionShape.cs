
// SPDX-License-Identifier: GPL-3.0-or-later


namespace SnapX.ScreenCaptureLib
{
    public abstract class BaseRegionShape : BaseShape
    {
        public override ShapeCategory ShapeCategory { get; } = ShapeCategory.Region;
    }
}