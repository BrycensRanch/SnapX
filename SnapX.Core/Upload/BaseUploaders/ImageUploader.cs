
// SPDX-License-Identifier: GPL-3.0-or-later


using SixLabors.ImageSharp;

namespace SnapX.Core.Upload.BaseUploaders
{
    public abstract class ImageUploader : FileUploader
    {
        public UploadResult UploadImage(Image image, string fileName)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // TODO: Actually save the image...
                // image.Save(stream, image.Metadata);

                return Upload(stream, fileName);
            }
        }
    }
}
