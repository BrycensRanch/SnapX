
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System;
using System.Drawing;

namespace SnapX.ScreenCaptureLib
{
    internal class ResizeNode : ImageEditorControl
    {
        public const int DefaultSize = 13;

        private PointF position;

        public PointF Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;

                Rectangle = new RectangleF(position.X - ((Size - 1) / 2), position.Y - ((Size - 1) / 2), Size, Size);
            }
        }

        public int Size { get; set; }

        public bool AutoSetSize { get; set; } = true;

        private NodeShape shape;

        public NodeShape Shape
        {
            get
            {
                return shape;
            }
            set
            {
                shape = value;

                if (AutoSetSize)
                {
                    if (shape == NodeShape.CustomNode && CustomNodeImage != null)
                    {
                        Size = Math.Max(CustomNodeImage.Width, CustomNodeImage.Height);
                    }
                    else
                    {
                        Size = DefaultSize;
                    }
                }
            }
        }

        public Image CustomNodeImage { get; private set; }

        public ResizeNode(float x = 0, float y = 0)
        {
            Shape = NodeShape.Square;
            Position = new PointF(x, y);
        }

        public void SetCustomNode(Image customNodeImage)
        {
            CustomNodeImage = customNodeImage;
            Shape = NodeShape.CustomNode;
        }

        public override void OnDraw(Graphics g)
        {
            RectangleF rect = Rectangle.SizeOffset(-1);

            switch (Shape)
            {
                case NodeShape.Square:
                    g.DrawRectangle(Pens.White, rect.Round().Offset(-1));
                    g.DrawRectangle(Pens.Black, rect.Round());
                    break;
                default:
                case NodeShape.Circle:
                    g.DrawEllipse(Pens.White, rect.Offset(-1));
                    g.DrawEllipse(Pens.Black, rect);
                    break;
                case NodeShape.Diamond:
                    g.DrawDiamond(Pens.White, rect.Round().Offset(-1));
                    g.DrawDiamond(Pens.Black, rect.Round());
                    break;
                case NodeShape.CustomNode when CustomNodeImage != null:
                    g.DrawImage(CustomNodeImage, Rectangle);
                    break;
            }
        }
    }
}