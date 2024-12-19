
// SPDX-License-Identifier: GPL-3.0-or-later


namespace ShareX.Core.Upload.BaseUploaders
{
    public abstract class GenericUploader : Uploader
    {
        public abstract UploadResult Upload(Stream stream, string fileName);
    }
}
