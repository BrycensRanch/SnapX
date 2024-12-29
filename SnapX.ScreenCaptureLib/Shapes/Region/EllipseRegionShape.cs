
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Drawing;
using System.Drawing.Drawing2D;

namespace SnapX.ScreenCaptureLib
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