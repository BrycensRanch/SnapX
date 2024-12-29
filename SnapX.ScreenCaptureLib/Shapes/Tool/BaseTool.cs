
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Drawing;

namespace SnapX.ScreenCaptureLib
{
    public abstract class BaseTool : BaseShape
    {
        public override ShapeCategory ShapeCategory { get; } = ShapeCategory.Tool;

        public virtual void OnDraw(Graphics g)
        {
        }
    }
}