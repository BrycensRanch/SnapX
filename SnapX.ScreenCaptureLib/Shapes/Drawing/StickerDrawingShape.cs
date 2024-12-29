
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.HelpersLib;
using System.Drawing;
using System.Windows.Forms;

namespace SnapX.ScreenCaptureLib
{
    public class StickerDrawingShape : ImageDrawingShape
    {
        public override ShapeType ShapeType { get; } = ShapeType.DrawingSticker;

        public override void OnConfigLoad()
        {
            ImageInterpolationMode = ImageInterpolationMode.NearestNeighbor;
        }

        public override void OnConfigSave()
        {
        }

        public override void ShowNodes()
        {
        }

        public override void OnCreating()
        {
            PointF pos = Manager.Form.ScaledClientMousePosition;
            Rectangle = new RectangleF(pos.X, pos.Y, 1, 1);

            if (Manager.IsCtrlModifier && LoadSticker(AnnotationOptions.LastStickerPath, AnnotationOptions.StickerSize))
            {
                OnCreated();
                Manager.IsMoving = true;
            }
            else if (OpenStickerForm())
            {
                OnCreated();
            }
            else
            {
                Remove();
            }
        }

        public override void OnDoubleClicked()
        {
            OpenStickerForm();
        }

        public override void Resize(int x, int y, bool fromBottomRight)
        {
            Move(x, y);
        }

        private bool OpenStickerForm()
        {
            Manager.Form.Pause();

            try
            {
                using (StickerForm stickerForm = new StickerForm(AnnotationOptions.StickerPacks, AnnotationOptions.SelectedStickerPack, AnnotationOptions.StickerSize))
                {
                    if (stickerForm.ShowDialog(Manager.Form) == DialogResult.OK)
                    {
                        AnnotationOptions.SelectedStickerPack = stickerForm.SelectedStickerPack;
                        AnnotationOptions.StickerSize = stickerForm.StickerSize;

                        return LoadSticker(stickerForm.SelectedImageFile, stickerForm.StickerSize);
                    }
                }
            }
            finally
            {
                Manager.Form.Resume();
            }

            return false;
        }

        private bool LoadSticker(string filePath, int stickerSize)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                Bitmap bmp = ImageHelpers.LoadImage(filePath);

                if (bmp != null)
                {
                    AnnotationOptions.LastStickerPath = filePath;

                    bmp = ImageHelpers.ResizeImageLimit(bmp, stickerSize);

                    SetImage(bmp, true);

                    return true;
                }
            }

            return false;
        }
    }
}