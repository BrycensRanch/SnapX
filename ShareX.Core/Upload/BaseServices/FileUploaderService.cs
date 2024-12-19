
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Utils;

namespace ShareX.Core.Upload.BaseServices
{
    public abstract class FileUploaderService : UploaderService<FileDestination>, IGenericUploaderService
    {
        public abstract GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo);
    }
}
