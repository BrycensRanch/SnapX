
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Drawing;

namespace SnapX.ScreenCaptureLib
{
    public class ImageFileDrawingShape : ImageDrawingShape
    {
        public override void OnCreating()
        {
            PointF pos = Manager.Form.ScaledClientMousePosition;
            Rectangle = new RectangleF(pos.X, pos.Y, 1, 1);

            if (Manager.IsCtrlModifier && LoadImageFile(AnnotationOptions.LastImageFilePath, true))
            {
                OnCreated();
                Manager.IsMoving = true;
            }
            else if (OpenImageDialog(true))
            {
                OnCreated();
                ShowNodes();
            }
            else
            {
                Remove();
            }
        }

        public override void OnDoubleClicked()
        {
            OpenImageDialog(false);
        }

        private bool OpenImageDialog(bool centerImage)
        {
            Manager.IsMoving = false;
            Manager.Form.Pause();
            string filePath = ImageHelpers.OpenImageFileDialog(Manager.Form);
            Manager.Form.Resume();
            return LoadImageFile(filePath, centerImage);
        }

        private bool LoadImageFile(string filePath, bool centerImage)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                Bitmap bmp = ImageHelpers.LoadImage(filePath);

                if (bmp != null)
                {
                    AnnotationOptions.LastImageFilePath = filePath;

                    SetImage(bmp, centerImage);

                    return true;
                }
            }

            return false;
        }
    }
}