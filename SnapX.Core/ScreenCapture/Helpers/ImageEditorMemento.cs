
// SPDX-License-Identifier: GPL-3.0-or-later


using SixLabors.ImageSharp;
using RectangleF = System.Drawing.RectangleF;

namespace SnapX.Core.ScreenCapture.Helpers
{
    internal class ImageEditorMemento : IDisposable
    {
        public List<BaseShape> Shapes { get; private set; }
        public RectangleF CanvasRectangle { get; private set; }
        public Image Canvas { get; private set; }

        public ImageEditorMemento(List<BaseShape> shapes, RectangleF canvasRectangle, Image canvas = null)
        {
            Shapes = shapes;
            CanvasRectangle = canvasRectangle;
            Canvas = canvas;
        }

        public void Dispose()
        {
            foreach (BaseShape shape in Shapes)
            {
                shape?.Dispose();
            }

            Shapes.Clear();

            Canvas?.Dispose();
        }
    }
}
