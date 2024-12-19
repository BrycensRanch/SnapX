
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Utils;

namespace ShareX.Core.Upload.File
{
    public class UguuFileUploaderService : FileUploaderService
    {
        public override FileDestination EnumValue { get; } = FileDestination.Uguu;

        public override bool CheckConfig(UploadersConfig config) => true;

        public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
        {
            return new Uguu();
        }
    }

    public class Uguu : FileUploader
    {
        public override UploadResult Upload(Stream stream, string fileName)
        {
            UploadResult result = SendRequestFile("https://uguu.se/upload.php?output=text", stream, fileName, "files[]");

            if (result.IsSuccess)
            {
                result.URL = result.Response;
            }

            return result;
        }
    }
}
