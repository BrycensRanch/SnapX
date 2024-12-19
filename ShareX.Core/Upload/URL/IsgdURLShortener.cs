
// SPDX-License-Identifier: GPL-3.0-or-later



using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Utils;

namespace ShareX.Core.Upload.URL;

public class IsgdURLShortenerService : URLShortenerService
{
    public override UrlShortenerType EnumValue { get; } = UrlShortenerType.ISGD;

    public override bool CheckConfig(UploadersConfig config) => true;

    public override URLShortener CreateShortener(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new IsgdURLShortener();
    }
}

public class IsgdURLShortener : URLShortener
{
    protected virtual string APIURL => "https://is.gd/create.php";

    public override UploadResult ShortenURL(string url)
    {
        UploadResult result = new UploadResult { URL = url };

        if (!string.IsNullOrEmpty(url))
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            arguments.Add("format", "simple");
            arguments.Add("url", url);

            result.Response = SendRequest(HttpMethod.Get, APIURL, arguments);

            if (!result.Response.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            {
                result.ShortenedURL = result.Response;
            }
        }

        return result;
    }
}

