
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Text.Json;
using ShareX.Core.Upload.BaseUploaders;

namespace ShareX.Core.Upload.Image;

public sealed class ImmioUploader : ImageUploader
{
    public override UploadResult Upload(Stream stream, string fileName)
    {
        var result = SendRequestFile("https://imm.io/store/", stream, fileName, "image");
        if (!result.IsSuccess) return result;

        var response = JsonSerializer.Deserialize<ImmioResponse>(result.Response);
        if (response != null) result.URL = response.Payload.Uri;
        return result;
    }

    private class ImmioResponse
    {
        public bool Success { get; set; }
        public ImmioPayload Payload { get; set; }
    }

    private class ImmioPayload
    {
        public string Uid { get; set; }
        public string Uri { get; set; }
        public string Link { get; set; }
        public string Name { get; set; }
        public string Format { get; set; }
        public string Ext { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Size { get; set; }
    }
}

