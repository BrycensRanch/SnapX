
// SPDX-License-Identifier: GPL-3.0-or-later


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SixLabors.ImageSharp;

namespace SnapX.ScreenCaptureLib.Helpers
{
    internal class ImageEditorHistory : IDisposable
    {
        public bool CanUndo => undoMementoStack.Count > 0;
        public bool CanRedo => redoMementoStack.Count > 0;

        private readonly ShapeManager shapeManager;
        private Stack<ImageEditorMemento> undoMementoStack = new();
        private Stack<ImageEditorMemento> redoMementoStack = new();

        public ImageEditorHistory(ShapeManager shapeManager)
        {
            this.shapeManager = shapeManager;
        }

        private void AddMemento(ImageEditorMemento memento)
        {
            undoMementoStack.Push(memento);

            foreach (ImageEditorMemento redoMemento in redoMementoStack)
            {
                redoMemento?.Dispose();
            }

            redoMementoStack.Clear();
        }

        private ImageEditorMemento GetMementoFromCanvas()
        {
            List<BaseShape> shapes = shapeManager.Shapes.Select(x => x.Duplicate()).ToList();
            Image canvas = shapeManager.Form.Canvas.Clone();
            return new ImageEditorMemento(shapes, shapeManager.Form.CanvasRectangle, canvas);
        }

        private ImageEditorMemento GetMementoFromShapes()
        {
            List<BaseShape> shapes = shapeManager.Shapes.Select(x => x.Duplicate()).ToList();
            return new ImageEditorMemento(shapes, shapeManager.Form.CanvasRectangle);
        }

        public void CreateCanvasMemento()
        {
            ImageEditorMemento memento = GetMementoFromCanvas();
            AddMemento(memento);
        }

        public void CreateShapesMemento()
        {
            if (!shapeManager.IsCurrentShapeTypeRegion && shapeManager.CurrentTool != ShapeType.ToolCrop && shapeManager.CurrentTool != ShapeType.ToolCutOut)
            {
                ImageEditorMemento memento = GetMementoFromShapes();
                AddMemento(memento);
            }
        }

        public void Undo()
        {
            if (CanUndo)
            {
                ImageEditorMemento undoMemento = undoMementoStack.Pop();

                if (undoMemento.Shapes != null)
                {
                    if (undoMemento.Canvas == null)
                    {
                        ImageEditorMemento redoMemento = GetMementoFromShapes();
                        redoMementoStack.Push(redoMemento);

                        shapeManager.RestoreState(undoMemento);
                    }
                    else
                    {
                        ImageEditorMemento redoMemento = GetMementoFromCanvas();
                        redoMementoStack.Push(redoMemento);

                        shapeManager.RestoreState(undoMemento);
                    }
                }
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                ImageEditorMemento redoMemento = redoMementoStack.Pop();

                if (redoMemento.Shapes != null)
                {
                    if (redoMemento.Canvas == null)
                    {
                        ImageEditorMemento undoMemento = GetMementoFromShapes();
                        undoMementoStack.Push(undoMemento);

                        shapeManager.RestoreState(redoMemento);
                    }
                    else
                    {
                        ImageEditorMemento undoMemento = GetMementoFromCanvas();
                        undoMementoStack.Push(undoMemento);

                        shapeManager.RestoreState(redoMemento);
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (ImageEditorMemento undoMemento in undoMementoStack)
            {
                undoMemento?.Dispose();
            }

            undoMementoStack.Clear();

            foreach (ImageEditorMemento redoMemento in redoMementoStack)
            {
                redoMemento?.Dispose();
            }

            redoMementoStack.Clear();
        }
    }
}
