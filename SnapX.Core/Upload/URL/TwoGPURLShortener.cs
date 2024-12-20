
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Text.Json;
using SnapX.Core.Upload.BaseServices;
using SnapX.Core.Upload.BaseUploaders;
using SnapX.Core.Upload.Utils;

namespace SnapX.Core.Upload.URL;

public class TwoGPURLShortenerService : URLShortenerService
{
    public override UrlShortenerType EnumValue => UrlShortenerType.TwoGP;

    public override bool CheckConfig(UploadersConfig config) => true;

    public override URLShortener CreateShortener(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new TwoGPURLShortener();
    }
}

public sealed class TwoGPURLShortener : URLShortener
{
    private const string API_ENDPOINT = "https://2.gp/api/short";

    public override UploadResult ShortenURL(string url)
    {
        var result = new UploadResult { URL = url };

        var args = new Dictionary<string, string> { { "longurl", url } };

        var response = SendRequest(HttpMethod.Get, API_ENDPOINT, args);

        if (string.IsNullOrEmpty(response))
            return result;

        var jsonResponse = JsonSerializer.Deserialize<TwoGPURLShortenerResponse>(response);

        if (jsonResponse != null)
        {
            result.ShortenedURL = jsonResponse.url;
        }

        return result;
    }
}

public class TwoGPURLShortenerResponse
{
    public string facebook_url { get; set; }
    public string stat_url { get; set; }
    public string twitter_url { get; set; }
    public string url { get; set; }
    public string target_host { get; set; }
    public string host { get; set; }
}

