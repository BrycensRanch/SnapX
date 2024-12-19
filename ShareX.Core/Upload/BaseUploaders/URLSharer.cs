
// SPDX-License-Identifier: GPL-3.0-or-later


namespace ShareX.Core.Upload.BaseUploaders
{
    public abstract class URLSharer : Uploader
    {
        public abstract UploadResult ShareURL(string url);
    }
}
