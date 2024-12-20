
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ShareX.ScreenCaptureLib
{
    public class FreehandRegionShape : BaseRegionShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.RegionFreehand;

        public PointF LastPosition
        {
            get
            {
                if (points.Count > 0)
                {
                    return points[points.Count - 1];
                }

                return PointF.Empty;
            }
            set
            {
                if (points.Count > 0)
                {
                    points[points.Count - 1] = value;
                }
            }
        }

        private List<PointF> points = new List<PointF>();
        private bool isPolygonMode;

        protected override void UseLightResizeNodes()
        {
            ChangeNodeShape(NodeShape.Circle);
        }

        public override void OnUpdate()
        {
            if (Manager.IsCreating)
            {
                if (Manager.IsCornerMoving)
                {
                    Move(Manager.Form.ScaledClientMouseVelocity);
                }
                else
                {
                    PointF pos = Manager.Form.ScaledClientMousePosition;

                    if (points.Count == 0 || (!Manager.IsProportionalResizing && LastPosition != pos))
                    {
                        points.Add(pos);
                    }

                    if (Manager.IsProportionalResizing)
                    {
                        if (!isPolygonMode)
                        {
                            points.Add(pos);
                        }

                        LastPosition = pos;
                    }

                    isPolygonMode = Manager.IsProportionalResizing;

                    Rectangle = points.CreateRectangle();
                }
            }
            else if (Manager.IsMoving)
            {
                Move(Manager.Form.ScaledClientMouseVelocity);
            }
        }

        public override void OnShapePathRequested(GraphicsPath gp, RectangleF rect)
        {
            if (points.Count > 2)
            {
                gp.AddPolygon(points.ToArray());
            }
            else if (points.Count == 2)
            {
                gp.AddLine(points[0], points[1]);
            }
        }

        public override void Move(float x, float y)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = points[i].Add(x, y);
            }

            Rectangle = Rectangle.LocationOffset(x, y);
        }

        public override void Resize(int x, int y, bool fromBottomRight)
        {
            Move(x, y);
        }

        public override void OnNodeVisible()
        {
            Manager.ResizeNodes[(int)NodePosition.TopLeft].Visible = true;
        }

        public override void OnNodeUpdate()
        {
            if (Manager.ResizeNodes[(int)NodePosition.TopLeft].IsDragging)
            {
                Manager.IsCreating = true;
                Manager.NodesVisible = false;
            }
        }

        public override void OnNodePositionUpdate()
        {
            Manager.ResizeNodes[(int)NodePosition.TopLeft].Position = LastPosition;
        }
    }
}