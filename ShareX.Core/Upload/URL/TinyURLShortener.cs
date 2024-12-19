
// SPDX-License-Identifier: GPL-3.0-or-later



using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Utils;

namespace ShareX.Core.Upload.URL;

public class TinyURLShortenerService : URLShortenerService
{
    public override UrlShortenerType EnumValue => UrlShortenerType.TINYURL;

    public override bool CheckConfig(UploadersConfig config) => true;

    public override URLShortener CreateShortener(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new TinyURLShortener();
    }
}

public sealed class TinyURLShortener : URLShortener
{
    public override UploadResult ShortenURL(string url)
    {
        var result = new UploadResult { URL = url };
        if (string.IsNullOrEmpty(url)) return result;

        var arguments = new Dictionary<string, string>
        {
            { "url", url }
        };

        result.Response = result.ShortenedURL = SendRequest(HttpMethod.Get, "https://tinyurl.com/api-create.php", arguments);

        return result;
    }
}

