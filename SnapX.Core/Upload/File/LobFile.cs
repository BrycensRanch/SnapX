
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using SnapX.Core.Upload.BaseServices;
using SnapX.Core.Upload.BaseUploaders;
using SnapX.Core.Upload.Utils;

namespace SnapX.Core.Upload.File;
public class LobFileFileUploaderService : FileUploaderService
{
    public override FileDestination EnumValue { get; } = FileDestination.Lithiio;


    public override bool CheckConfig(UploadersConfig config)
    {
        return config.LithiioSettings != null && !string.IsNullOrEmpty(config.LithiioSettings.UserAPIKey);
    }

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new LobFile(config.LithiioSettings);
    }
}

public sealed class LobFile : FileUploader
{
    public LobFileSettings Config { get; private set; }

    public LobFile()
    {
    }

    public LobFile(LobFileSettings config)
    {
        Config = config;
    }

    [RequiresDynamicCode("Uploader")]
    [RequiresUnreferencedCode("Uploader")]
    public override UploadResult Upload(Stream stream, string fileName)
    {
        Dictionary<string, string> args = new Dictionary<string, string>();
        args.Add("api_key", Config.UserAPIKey);

        UploadResult result = SendRequestFile("https://lobfile.com/api/v3/upload", stream, fileName, "file", args);

        if (result.IsSuccess)
        {
            LobFileUploadResponse uploadResponse = JsonSerializer.Deserialize<LobFileUploadResponse>(result.Response);

            if (uploadResponse.Success)
            {
                result.URL = uploadResponse.URL;
            }
            else
            {
                Errors.Add(uploadResponse.Error);
            }
        }

        return result;
    }

    [RequiresDynamicCode("Uploader")]
    [RequiresUnreferencedCode("Uploader")]
    public string FetchAPIKey(string email, string password)
    {
        var args = new Dictionary<string, string>
        {
            { "email", email },
            { "password", password }
        };

        var response = SendRequestMultiPart("https://lobfile.com/api/v3/fetch-api-key", args);

        if (string.IsNullOrEmpty(response)) return null;

        var apiKeyResponse = JsonSerializer.Deserialize<LobFileFetchAPIKeyResponse>(response);

        if (apiKeyResponse?.Success ?? false)
            return apiKeyResponse.API_Key;

        throw new Exception(apiKeyResponse?.Error ?? "Unknown error");
    }


    private class LobFileResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    private class LobFileUploadResponse : LobFileResponse
    {
        public string URL { get; set; }
    }

    private class LobFileFetchAPIKeyResponse : LobFileResponse
    {
        public string API_Key { get; set; }
    }
}

public class LobFileSettings
{
    public string UserAPIKey { get; set; } = "";
}

