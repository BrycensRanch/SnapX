
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Windows.Forms;

namespace ShareX.ScreenCaptureLib
{
    internal class ScrollbarManager
    {
        public bool Visible => horizontalScrollbar.Visible || verticalScrollbar.Visible;

        private RegionCaptureForm form;
        private ImageEditorScrollbar horizontalScrollbar, verticalScrollbar;

        public ScrollbarManager(RegionCaptureForm regionCaptureForm, ShapeManager shapeManager)
        {
            form = regionCaptureForm;
            horizontalScrollbar = new ImageEditorScrollbar(Orientation.Horizontal, form);
            shapeManager.DrawableObjects.Add(horizontalScrollbar);
            verticalScrollbar = new ImageEditorScrollbar(Orientation.Vertical, form);
            shapeManager.DrawableObjects.Add(verticalScrollbar);
        }

        public void Update()
        {
            horizontalScrollbar.Update();
            verticalScrollbar.Update();
        }
    }
}