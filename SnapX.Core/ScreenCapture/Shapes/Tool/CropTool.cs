
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Drawing;

namespace SnapX.Core.ScreenCapture.Shapes.Tool
{
    public class CropTool : BaseTool
    {
        public override ShapeType ShapeType { get; } = ShapeType.ToolCrop;

        public override bool LimitRectangleToInsideCanvas { get; } = true;

        private ImageEditorButton confirmButton, cancelButton;
        private Size buttonSize = new Size(80, 40);
        private int buttonOffset = 15;

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (confirmButton != null && cancelButton != null)
            {
                if (Rectangle.Bottom + buttonOffset + buttonSize.Height > Manager.Form.ClientArea.Bottom &&
                    Rectangle.Width > (buttonSize.Width * 2) + (buttonOffset * 3) &&
                    Rectangle.Height > buttonSize.Height + (buttonOffset * 2))
                {
                    confirmButton.Rectangle = new RectangleF(Rectangle.Right - (buttonOffset * 2) - (buttonSize.Width * 2),
                        Rectangle.Bottom - buttonOffset - buttonSize.Height, buttonSize.Width, buttonSize.Height);
                    cancelButton.Rectangle = new RectangleF(Rectangle.Right - buttonOffset - buttonSize.Width,
                        Rectangle.Bottom - buttonOffset - buttonSize.Height, buttonSize.Width, buttonSize.Height);
                }
                else
                {
                    confirmButton.Rectangle = new RectangleF(Rectangle.Right - (buttonSize.Width * 2) - buttonOffset,
                        Rectangle.Bottom + buttonOffset, buttonSize.Width, buttonSize.Height);
                    cancelButton.Rectangle = new RectangleF(Rectangle.Right - buttonSize.Width,
                        Rectangle.Bottom + buttonOffset, buttonSize.Width, buttonSize.Height);
                }
            }
        }

        public override void OnDraw(Graphics g)
        {
            if (IsValidShape)
            {
                Manager.DrawRegionArea(g, Rectangle, true, Manager.Options.ShowInfo);
                g.DrawCross(Pens.Black, Rectangle.Center(), 10);
            }
        }

        public override void OnCreated()
        {
            confirmButton = new ImageEditorButton()
            {
                Text = "\u2714",
                ButtonColor = Color.ForestGreen,
                Rectangle = new Rectangle(new Point(), buttonSize),
                Visible = true
            };
            confirmButton.MouseDown += ConfirmButton_MousePressed;
            confirmButton.MouseEnter += () => Manager.Form.Cursor = Cursors.Hand;
            confirmButton.MouseLeave += () => Manager.Form.SetDefaultCursor();
            Manager.DrawableObjects.Add(confirmButton);

            cancelButton = new ImageEditorButton()
            {
                Text = "\u2716",
                ButtonColor = Color.FromArgb(227, 45, 45),
                Rectangle = new Rectangle(new Point(), buttonSize),
                Visible = true
            };
            cancelButton.MouseDown += CancelButton_MousePressed;
            cancelButton.MouseEnter += () => Manager.Form.Cursor = Cursors.Hand;
            cancelButton.MouseLeave += () => Manager.Form.SetDefaultCursor();
            Manager.DrawableObjects.Add(cancelButton);
        }

        private void ConfirmButton_MousePressed(object sender, MouseEventArgs e)
        {
            Manager.CropArea(Rectangle);
            Remove();
        }

        private void CancelButton_MousePressed(object sender, MouseEventArgs e)
        {
            Remove();
        }

        public override void Remove()
        {
            base.Remove();

            if (Options.SwitchToSelectionToolAfterDrawing)
            {
                Manager.CurrentTool = ShapeType.ToolSelect;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if ((confirmButton != null && confirmButton.IsCursorHover) || (cancelButton != null && cancelButton.IsCursorHover))
            {
                Manager.Form.SetDefaultCursor();
            }

            if (confirmButton != null)
            {
                Manager.DrawableObjects.Remove(confirmButton);
            }

            if (cancelButton != null)
            {
                Manager.DrawableObjects.Remove(cancelButton);
            }
        }
    }
}
