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
using System.Net;
using System.Net.Cache;
using System.Text;
using ShareX.Core.Utils.Cryptographic;
using ShareX.Core.Utils.Miscellaneous;

namespace ShareX.Core.Upload.Utils;

internal static class RequestHelpers
{
    public const string ContentTypeMultipartFormData = "multipart/form-data";
    public const string ContentTypeJSON = "application/json";
    public const string ContentTypeXML = "application/xml";
    public const string ContentTypeURLEncoded = "application/x-www-form-urlencoded";
    public const string ContentTypeOctetStream = "application/octet-stream";

    public static HttpWebRequest CreateWebRequest(HttpMethod method, string url, NameValueCollection headers = null, CookieCollection cookies = null,
        string contentType = null, long contentLength = 0)
    {
        var request = (HttpWebRequest)WebRequest.Create(url);

        string accept = null;
        string referer = null;
        var userAgent = ShareXResources.UserAgent;

        if (headers != null)
        {
            if (headers["Accept"] != null)
            {
                accept = headers["Accept"];
                headers.Remove("Accept");
            }

            if (headers["Content-Length"] != null)
            {
                if (long.TryParse(headers["Content-Length"], out contentLength))
                {
                    request.ContentLength = contentLength;
                }

                headers.Remove("Content-Length");
            }

            if (headers["Content-Type"] != null)
            {
                contentType = headers["Content-Type"];
                headers.Remove("Content-Type");
            }

            if (headers["Cookie"] != null)
            {
                var cookieHeader = headers["Cookie"];

                if (cookies == null)
                {
                    cookies = new CookieCollection();
                }

                foreach (var cookie in cookieHeader.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var cookieValues = cookie.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                    if (cookieValues.Length == 2)
                    {
                        cookies.Add(new Cookie(cookieValues[0], cookieValues[1], "/", request.Host.Split(':')[0]));
                    }
                }

                headers.Remove("Cookie");
            }

            if (headers["Referer"] != null)
            {
                referer = headers["Referer"];
                headers.Remove("Referer");
            }

            if (headers["User-Agent"] != null)
            {
                userAgent = headers["User-Agent"];
                headers.Remove("User-Agent");
            }

            request.Headers.Add(headers);
        }

        request.Accept = accept;
        request.ContentType = contentType;
        request.CookieContainer = new CookieContainer();
        if (cookies != null) request.CookieContainer.Add(cookies);
        request.Method = method.ToString();
        var proxy = HelpersOptions.CurrentProxy.GetWebProxy();
        if (proxy != null) request.Proxy = proxy;
        request.Referer = referer;
        request.UserAgent = userAgent;

        if (contentLength > 0)
        {
            request.AllowWriteStreamBuffering = HelpersOptions.CurrentProxy.IsValidProxy();

            if (method == HttpMethod.Get)
            {
                request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            }

            request.ContentLength = contentLength;
            request.Pipelined = false;
            request.Timeout = -1;
        }
        else
        {
            request.KeepAlive = false;
        }

        return request;
    }

    public static string CreateBoundary()
    {
        return new string('-', 20) + DateTime.Now.Ticks.ToString("x");
    }

    public static byte[] MakeInputContent(string boundary, string name, string value)
    {
        var content = $"--{boundary}\r\nContent-Disposition: form-data; name=\"{name}\"\r\n\r\n{value}\r\n";
        return Encoding.UTF8.GetBytes(content);
    }

    public static byte[] MakeInputContent(string boundary, Dictionary<string, string> contents, bool isFinal = true)
    {
        if (string.IsNullOrEmpty(boundary))
            boundary = CreateBoundary();

        if (contents == null || contents.Count == 0)
            return Array.Empty<byte>();

        using var stream = new MemoryStream();
        foreach (var content in contents.Where(c => !string.IsNullOrEmpty(c.Key)))
        {
            var bytes = MakeInputContent(boundary, content.Key, content.Value);
            stream.Write(bytes, 0, bytes.Length);
        }

        if (isFinal)
        {
            var bytes = Encoding.UTF8.GetBytes($"--{boundary}--\r\n");
            stream.Write(bytes, 0, bytes.Length);
        }

        return stream.ToArray();
    }


    public static byte[] MakeFileInputContentOpen(string boundary, string fileFormName, string fileName)
    {
        var mimeType = MimeTypes.GetMimeTypeFromFileName(fileName);
        var content = $"--{boundary}\r\nContent-Disposition: form-data; name=\"{fileFormName}\"; filename=\"{fileName}\"\r\nContent-Type: {mimeType}\r\n\r\n";
        return Encoding.UTF8.GetBytes(content);
    }

    public static byte[] MakeRelatedFileInputContentOpen(string boundary, string contentType, string relatedData, string fileName)
    {
        var mimeType = MimeTypes.GetMimeTypeFromFileName(fileName);
        var content = $"--{boundary}\r\nContent-Type: {contentType}\r\n\r\n{relatedData}\r\n\r\n";
        content += $"--{boundary}\r\nContent-Type: {mimeType}\r\n\r\n";
        return Encoding.UTF8.GetBytes(content);
    }

    public static byte[] MakeFileInputContentClose(string boundary)
    {
        return Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");
    }

    public static string ResponseToString(WebResponse response)
    {
        using var responseStream = response?.GetResponseStream();
        if (responseStream == null) return null;

        using var reader = new StreamReader(responseStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }


    public static NameValueCollection CreateAuthenticationHeader(string username, string password)
    {
        var authorization = TranslatorHelper.TextToBase64($"{username}:{password}");
        return new NameValueCollection
        {
            ["Authorization"] = $"Basic {authorization}"
        };
    }
}

