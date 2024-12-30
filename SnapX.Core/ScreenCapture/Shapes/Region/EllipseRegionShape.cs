
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Drawing;

namespace SnapX.Core.ScreenCapture.Shapes.Region
{
    public class EllipseRegionShape : BaseRegionShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.RegionEllipse;

        public override void OnShapePathRequested(GraphicsPath gp, RectangleF rect)
        {
            gp.AddEllipse(rect);
        }
    }
}
