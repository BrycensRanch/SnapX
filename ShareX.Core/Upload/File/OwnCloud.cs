
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Collections.Specialized;
using System.Text.Json;
using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Utils;
using ShareX.Core.Utils;
using ShareX.Core.Utils.Miscellaneous;

namespace ShareX.Core.Upload.File;

public class OwnCloudFileUploaderService : FileUploaderService
{
    public override FileDestination EnumValue => FileDestination.OwnCloud;

    public override bool CheckConfig(UploadersConfig config)
    {
        return !string.IsNullOrEmpty(config.OwnCloudHost) && !string.IsNullOrEmpty(config.OwnCloudUsername) && !string.IsNullOrEmpty(config.OwnCloudPassword);
    }

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new OwnCloud(config.OwnCloudHost, config.OwnCloudUsername, config.OwnCloudPassword)
        {
            Path = config.OwnCloudPath,
            CreateShare = config.OwnCloudCreateShare,
            DirectLink = config.OwnCloudDirectLink,
            PreviewLink = config.OwnCloudUsePreviewLinks,
            AppendFileNameToURL = config.OwnCloudAppendFileNameToURL,
            IsCompatibility81 = config.OwnCloud81Compatibility,
            AutoExpireTime = config.OwnCloudExpiryTime,
            AutoExpire = config.OwnCloudAutoExpire
        };
    }
}

public sealed class OwnCloud : FileUploader
{
    public string Host { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Path { get; set; }
    public int AutoExpireTime { get; set; }
    public bool CreateShare { get; set; }
    public bool AppendFileNameToURL { get; set; }
    public bool DirectLink { get; set; }
    public bool PreviewLink { get; set; }
    public bool IsCompatibility81 { get; set; }
    public bool AutoExpire { get; set; }

    public OwnCloud(string host, string username, string password)
    {
        Host = host;
        Username = username;
        Password = password;
    }

    public override UploadResult Upload(Stream stream, string fileName)
    {
        if (string.IsNullOrEmpty(Host))
        {
            throw new Exception("ownCloud Host is empty.");
        }

        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            throw new Exception("ownCloud Username or Password is empty.");
        }

        if (string.IsNullOrEmpty(Path))
        {
            Path = "/";
        }

        // Original, unencoded path. Necessary for shared files
        string path = URLHelpers.CombineURL(Path, fileName);
        // Encoded path, necessary when sent in the URL
        string encodedPath = URLHelpers.CombineURL(Path, URLHelpers.URLEncode(fileName));

        string url = URLHelpers.CombineURL(Host, "remote.php/webdav", encodedPath);
        url = URLHelpers.FixPrefix(url);

        NameValueCollection headers = RequestHelpers.CreateAuthenticationHeader(Username, Password);
        headers["OCS-APIREQUEST"] = "true";

        string response = SendRequest(HttpMethod.Put, url, stream, MimeTypes.GetMimeTypeFromFileName(fileName), null, headers);

        UploadResult result = new UploadResult(response);

        if (!IsError)
        {
            if (CreateShare)
            {
                AllowReportProgress = false;
                result.URL = ShareFile(path, fileName);
            }
            else
            {
                result.IsURLExpected = false;
            }
        }

        return result;
    }

    // https://doc.owncloud.org/server/10.0/developer_manual/core/ocs-share-api.html#create-a-new-share
    public string ShareFile(string path, string fileName)
    {
        var args = new Dictionary<string, string>
        {
            { "path", path },
            { "shareType", "3" },
            { "permissions", "1" }
        };

        if (AutoExpire && AutoExpireTime != 0)
        {
            try
            {
                var expireTime = DateTime.UtcNow.AddDays(AutoExpireTime);
                args.Add("expireDate", $"{expireTime:yyyy-MM-dd}");
            }
            catch
            {
                throw new Exception("ownCloud Auto Expire time is invalid");
            }
        }
        else if (AutoExpire)
        {
            throw new Exception("ownCloud Auto Expire Time is not valid.");
        }

        var url = URLHelpers.CombineURL(Host, "ocs/v1.php/apps/files_sharing/api/v1/shares?format=json");
        url = URLHelpers.FixPrefix(url);

        var headers = RequestHelpers.CreateAuthenticationHeader(Username, Password);
        headers["OCS-APIREQUEST"] = "true";

        var response = SendRequestMultiPart(url, args, headers);
        if (string.IsNullOrEmpty(response)) return null;

        var result = JsonSerializer.Deserialize<OwnCloudShareResponse>(response);
        if (result?.ocs?.meta?.statuscode != 100 || result.ocs.data == null) return null;

        var data = JsonSerializer.Deserialize<OwnCloudShareResponseData>(result.ocs.data.ToString());
        var link = data.url;

        if (PreviewLink && FileHelpers.IsImageFile(path))
            return link + "/preview";

        if (DirectLink)
        {
            link += IsCompatibility81 ? "/download" : "&download";
            if (AppendFileNameToURL)
                link = URLHelpers.CombineURL(link, URLHelpers.URLEncode(fileName));
        }

        return link;
    }


    public class OwnCloudShareResponse
    {
        public OwnCloudShareResponseOcs ocs { get; set; }
    }

    public class OwnCloudShareResponseOcs
    {
        public OwnCloudShareResponseMeta meta { get; set; }
        public object data { get; set; }
    }

    public class OwnCloudShareResponseMeta
    {
        public string status { get; set; }
        public int statuscode { get; set; }
        public string message { get; set; }
    }

    public class OwnCloudShareResponseData
    {
        public int id { get; set; }
        public string url { get; set; }
        public string token { get; set; }
    }
}

