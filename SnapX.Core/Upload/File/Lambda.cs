
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using SnapX.Core.Upload.BaseServices;
using SnapX.Core.Upload.BaseUploaders;
using SnapX.Core.Upload.Utils;

namespace SnapX.Core.Upload.File;

public class LambdaFileUploaderService : FileUploaderService
{
    public override FileDestination EnumValue { get; } = FileDestination.Lambda;

    public override bool CheckConfig(UploadersConfig config)
    {
        return config.LambdaSettings != null && !string.IsNullOrEmpty(config.LambdaSettings.UserAPIKey);
    }

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        // Correct old URLs
        if (config.LambdaSettings != null && config.LambdaSettings.UploadURL == "https://λ.pw/")
        {
            config.LambdaSettings.UploadURL = "https://lbda.net/";
        }

        return new Lambda(config.LambdaSettings);
    }
}

public sealed class Lambda : FileUploader
{
    public LambdaSettings Config { get; private set; }

    public Lambda(LambdaSettings config)
    {
        Config = config;
    }

    private const string uploadUrl = "https://lbda.net/api/upload";

    public static string[] UploadURLs = ["https://lbda.net/", "https://lambda.sx/"];

    [RequiresDynamicCode("Uploader")]
    [RequiresUnreferencedCode("Uploader")]
    public override UploadResult Upload(Stream stream, string fileName)
    {
        Dictionary<string, string> arguments = new Dictionary<string, string>
        {
            { "api_key", Config.UserAPIKey }
        };
        UploadResult result = SendRequestFile(uploadUrl, stream, fileName, "file", arguments, method: HttpMethod.Put);

        if (result.Response == null)
        {
            Errors.Add("Upload failed for unknown reason. Check your API key.");
            return result;
        }

        LambdaResponse response = JsonSerializer.Deserialize<LambdaResponse>(result.Response);
        if (result.IsSuccess)
        {
            result.URL = Config.UploadURL + response.url;
        }
        else
        {
            foreach (string e in response.errors)
            {
                Errors.Add(e);
            }
        }

        return result;
    }

    internal class LambdaResponse
    {
        public string url { get; set; }
        public List<string> errors { get; set; }
    }

    internal class LambdaFile
    {
        public string url { get; set; }
    }
}

public class LambdaSettings
{
    public string UserAPIKey { get; set; } = "";
    public string UploadURL { get; set; } = "https://lbda.net/";
}

