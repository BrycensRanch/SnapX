
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Text.Json;
using SnapX.Core.Upload.BaseServices;
using SnapX.Core.Upload.BaseUploaders;
using SnapX.Core.Upload.Utils;
using SnapX.Core.Utils;

namespace SnapX.Core.Upload.Img;

public class CheveretoImageUploaderService : ImageUploaderService
{
    public override ImageDestination EnumValue => ImageDestination.Chevereto;
    public override bool CheckConfig(UploadersConfig config)
    {
        return config.CheveretoUploader != null && !string.IsNullOrEmpty(config.CheveretoUploader.UploadURL) &&
            !string.IsNullOrEmpty(config.CheveretoUploader.APIKey);
    }

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new Chevereto(config.CheveretoUploader)
        {
            DirectURL = config.CheveretoDirectURL
        };
    }
}

public sealed class Chevereto : ImageUploader
{
    public CheveretoUploader Uploader { get; private set; }

    public bool DirectURL { get; set; }

    public Chevereto(CheveretoUploader uploader)
    {
        Uploader = uploader;
    }

    public override UploadResult Upload(Stream stream, string fileName)
    {
        var args = new Dictionary<string, string>
        {
            { "key", Uploader.APIKey },
            { "format", "json" }
        };

        string url = URLHelpers.FixPrefix(Uploader.UploadURL);

        var result = SendRequestFile(url, stream, fileName, "source", args);

        if (!result.IsSuccess)
        {
            return result;
        }

        var response = JsonSerializer.Deserialize<CheveretoResponse>(result.Response);

        if (response?.Image == null)
        {
            return result;
        }

        result.URL = DirectURL ? response.Image.URL : response.Image.URL_Viewer;

        if (response.Image.Thumb?.URL != null)
        {
            result.ThumbnailURL = response.Image.Thumb.URL;
        }

        return result;
    }


    private class CheveretoResponse
    {
        public CheveretoImage Image { get; set; }
    }

    private class CheveretoImage
    {
        public string URL { get; set; }
        public string URL_Viewer { get; set; }
        public CheveretoThumb Thumb { get; set; }
    }

    private class CheveretoThumb
    {
        public string URL { get; set; }
    }
}

