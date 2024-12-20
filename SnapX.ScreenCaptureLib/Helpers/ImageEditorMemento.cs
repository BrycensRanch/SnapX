
// SPDX-License-Identifier: GPL-3.0-or-later


using System;
using System.Collections.Generic;
using System.Drawing;

namespace ShareX.ScreenCaptureLib
{
    internal class ImageEditorMemento : IDisposable
    {
        public List<BaseShape> Shapes { get; private set; }
        public RectangleF CanvasRectangle { get; private set; }
        public Bitmap Canvas { get; private set; }

        public ImageEditorMemento(List<BaseShape> shapes, RectangleF canvasRectangle, Bitmap canvas = null)
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