
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Drawing;

namespace ShareX.ScreenCaptureLib
{
    public class SmartEraserDrawingShape : BaseDrawingShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.DrawingSmartEraser;

        private Color eraserColor;
        private Color eraserDimmedColor;

        public override void OnConfigLoad()
        {
        }

        public override void OnConfigSave()
        {
        }

        public override void OnCreating()
        {
            base.OnCreating();

            eraserColor = Manager.GetCurrentColor();

            if (eraserColor.IsEmpty)
            {
                eraserColor = Color.White;
            }

            if (Manager.Form.DimmedCanvas != null)
            {
                eraserDimmedColor = Manager.GetCurrentColor(Manager.Form.DimmedCanvas);
            }
        }

        public override void OnDraw(Graphics g)
        {
            Color color;

            if (!Manager.IsRenderingOutput && !eraserDimmedColor.IsEmpty)
            {
                color = eraserDimmedColor;
            }
            else
            {
                color = eraserColor;
            }

            using (Brush brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, Rectangle);
            }
        }
    }
}