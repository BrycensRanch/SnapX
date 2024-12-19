
// SPDX-License-Identifier: GPL-3.0-or-later


namespace ShareX.ScreenCaptureLib
{
    public abstract class BaseRegionShape : BaseShape
    {
        public override ShapeCategory ShapeCategory { get; } = ShapeCategory.Region;
    }
}