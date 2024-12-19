
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Text.Json;
using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Utils;

namespace ShareX.Core.Upload.Image
{
    public class VgymeImageUploaderService : ImageUploaderService
    {
        public override ImageDestination EnumValue { get; } = ImageDestination.Vgyme;

        public override bool CheckConfig(UploadersConfig config) => true;

        public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
        {
            return new VgymeUploader()
            {
                UserKey = config.VgymeUserKey
            };
        }
    }

    public sealed class VgymeUploader : ImageUploader
    {
        public string UserKey { get; set; }

        public override UploadResult Upload(Stream stream, string fileName)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(UserKey)) args.Add("userkey", UserKey);

            UploadResult result = SendRequestFile("https://vgy.me/upload", stream, fileName, "file", args);

            if (result.IsSuccess)
            {
                VgymeResponse response = JsonSerializer.Deserialize<VgymeResponse>(result.Response);

                if (response != null && !response.Error)
                {
                    result.URL = response.Image;
                    result.DeletionURL = response.Delete;
                }
            }

            return result;
        }

        private class VgymeResponse
        {
            public bool Error { get; set; }
            public string URL { get; set; }
            public string Image { get; set; }
            public long Size { get; set; }
            public string Filename { get; set; }
            public string Ext { get; set; }
            public string Delete { get; set; }
        }
    }
}
