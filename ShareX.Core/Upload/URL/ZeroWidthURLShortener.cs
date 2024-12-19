
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Collections.Specialized;
using System.Text.Json;
using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Utils;
using ShareX.Core.Utils;

namespace ShareX.Core.Upload.URL;

public class ZeroWidthURLShortenerService : URLShortenerService
{
    public override UrlShortenerType EnumValue => UrlShortenerType.ZeroWidthShortener;
    public override bool CheckConfig(UploadersConfig config) => true;

    public override URLShortener CreateShortener(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new ZeroWidthURLShortener()
        {
            RequestURL = config.ZeroWidthShortenerURL,
            Token = config.ZeroWidthShortenerToken
        };
    }

}

public sealed class ZeroWidthURLShortener : URLShortener
{
    public string RequestURL { get; set; }
    public string Token { get; set; }

    private NameValueCollection GetAuthHeaders()
    {
        return string.IsNullOrEmpty(Token)
            ? null
            : new NameValueCollection { { "Authorization", "Bearer " + Token } };
    }


    public override UploadResult ShortenURL(string url)
    {
        var result = new UploadResult { URL = url };

        if (string.IsNullOrEmpty(url)) return result;

        var json = JsonSerializer.Serialize(new { url });

        RequestURL ??= "https://api.zws.im"; // Use null-coalescing assignment

        var headers = GetAuthHeaders();

        var response = SendRequest(HttpMethod.Post, RequestURL, json, RequestHelpers.ContentTypeJSON, null, headers);

        if (string.IsNullOrEmpty(response)) return result;

        var jsonResponse = JsonSerializer.Deserialize<ZeroWidthURLShortenerResponse>(response);

        if (jsonResponse?.URL != null)
        {
            result.ShortenedURL = jsonResponse.URL;
        }
        else
        {
            result.ShortenedURL = URLHelpers.CombineURL("https://zws.im", jsonResponse?.Short);
        }

        return result;
    }
}

public class ZeroWidthURLShortenerResponse
{
    public string Short { get; set; }
    public string URL { get; set; }
}

