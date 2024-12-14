#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2024 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

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

