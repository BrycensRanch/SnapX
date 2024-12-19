
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Utils;

namespace ShareX.Core.Upload.BaseServices
{
    public abstract class URLSharingService : UploaderService<URLSharingServices>
    {
        public abstract URLSharer CreateSharer(UploadersConfig config, TaskReferenceHelper taskInfo);
    }
}
