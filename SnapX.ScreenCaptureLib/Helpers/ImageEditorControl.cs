
// SPDX-License-Identifier: GPL-3.0-or-later


using System;
using System.Drawing;
using System.Windows.Forms;

namespace SnapX.ScreenCaptureLib
{
    internal abstract class ImageEditorControl
    {
        public event MouseEventHandler MouseDown, MouseUp;
        public event Action MouseEnter, MouseLeave;

        public bool Visible { get; set; }
        public bool HandleMouseInput { get; set; } = true;
        public RectangleF Rectangle { get; set; }

        private bool isCursorHover;

        public bool IsCursorHover
        {
            get
            {
                return isCursorHover;
            }
            set
            {
                if (isCursorHover != value)
                {
                    isCursorHover = value;

                    if (isCursorHover)
                    {
                        OnMouseEnter();
                    }
                    else
                    {
                        OnMouseLeave();
                    }
                }
            }
        }

        public bool IsDragging { get; protected set; }
        public int Order { get; set; }

        public virtual void OnDraw(Graphics g)
        {
            if (IsDragging)
            {
                g.FillRectangle(Brushes.Blue, Rectangle);
            }
            else if (IsCursorHover)
            {
                g.FillRectangle(Brushes.Green, Rectangle);
            }
            else
            {
                g.FillRectangle(Brushes.Red, Rectangle);
            }
        }

        public virtual void OnMouseEnter()
        {
            MouseEnter?.Invoke();
        }

        public virtual void OnMouseLeave()
        {
            MouseLeave?.Invoke();
        }

        public virtual void OnMouseDown(Point position)
        {
            IsDragging = true;

            MouseDown?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, position.X, position.Y, 0));
        }

        public virtual void OnMouseUp(Point position)
        {
            IsDragging = false;

            MouseUp?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, position.X, position.Y, 0));
        }
    }
}