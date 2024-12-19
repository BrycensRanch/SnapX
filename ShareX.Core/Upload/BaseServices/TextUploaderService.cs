
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Utils;

namespace ShareX.Core.Upload.BaseServices
{
    public abstract class TextUploaderService : UploaderService<TextDestination>, IGenericUploaderService
    {
        public abstract GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo);
    }
}
