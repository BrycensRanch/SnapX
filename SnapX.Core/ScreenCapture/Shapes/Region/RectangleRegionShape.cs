
// SPDX-License-Identifier: GPL-3.0-or-later



namespace SnapX.Core.ScreenCapture.Shapes.Region
{
    public class RectangleRegionShape : BaseRegionShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.RegionRectangle;

        public int CornerRadius { get; set; }

        public override void OnConfigLoad()
        {
            CornerRadius = AnnotationOptions.RegionCornerRadius;
        }

        public override void OnConfigSave()
        {
            AnnotationOptions.RegionCornerRadius = CornerRadius;
        }

        public override void OnShapePathRequested(GraphicsPath gp, RectangleF rect)
        {
            gp.AddRoundedRectangle(rect, CornerRadius);
        }
    }
}
