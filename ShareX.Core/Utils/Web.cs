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

using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using ShareX.Core.Utils.Miscellaneous;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ShareX.Core.Utils;

public static class WebHelpers
{
    public static async System.Threading.Tasks.Task DownloadFileAsync(string url, string filePath)
    {
        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(filePath))
        {
            return;
        }

        FileHelpers.CreateDirectoryFromFilePath(filePath);

        using var client = HttpClientFactory.Create();
        using var responseMessage = await client.GetAsync(url);

        if (!responseMessage.IsSuccessStatusCode)
        {
            return;
        }

        await using var responseStream = await responseMessage.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

        await responseStream.CopyToAsync(fileStream);
    }

    public static async System.Threading.Tasks.Task<SixLabors.ImageSharp.Image<Rgba64>> DataURLToImage(string url)
    {
        // Ensure the URL is valid and starts with "data:"
        if (url == null || !url.ToString().StartsWith("data:"))
        {
            throw new ArgumentException("Invalid data URL.");
        }

        // Extract the base64 data from the data URL
        var dataUrl = url.ToString();
        var regex = new Regex(@"^data:image\/(?<type>.*?);base64,(?<data>.+)$");
        var match = regex.Match(dataUrl);

        if (!match.Success)
        {
            throw new ArgumentException("Invalid data URL format.");
        }

        var base64Data = match.Groups["data"].Value;

        byte[] imageBytes = Convert.FromBase64String(base64Data);

        using var ms = new MemoryStream(imageBytes);
        var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba64>(ms);
        return image;
    }

    public static async Task<string> DownloadStringAsync(string url)
    {
        if (string.IsNullOrEmpty(url)) {
            return null;
        }

        using var client = HttpClientFactory.Create();
        using var responseMessage = await client.GetAsync(url);

        return responseMessage.IsSuccessStatusCode
            ? await responseMessage.Content.ReadAsStringAsync()
            : null;
    }



    public static async Task<string> GetFileNameFromWebServerAsync(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;

        using var client = HttpClientFactory.Create();
        using var requestMessage = new HttpRequestMessage(HttpMethod.Head, url);

        using var responseMessage = await client.SendAsync(requestMessage);

        return responseMessage.Content.Headers.ContentDisposition?.FileName;
    }


    public static async Task<Image<Rgba64>> DownloadImageAsync(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;

        using var client = HttpClientFactory.Create();

        using var responseMessage = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        if (!responseMessage.IsSuccessStatusCode || responseMessage.Content.Headers.ContentType == null)
            return null;

        var mediaType = responseMessage.Content.Headers.ContentType.MediaType;
        if (mediaType == null) return null;
        if (!MimeTypes.IsImageMimeType(mediaType))
            return null;

        var data = await responseMessage.Content.ReadAsByteArrayAsync();

        try
        {
            using var memoryStream = new MemoryStream(data);
            return await SixLabors.ImageSharp.Image.LoadAsync<Rgba64>(memoryStream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading image: {ex.Message}");
            return null;
        }
    }

    public static bool IsSuccessStatusCode(HttpStatusCode statusCode)
    {
        var statusCodeNum = (int)statusCode;
        return statusCodeNum >= 200 && statusCodeNum <= 299;
    }

    public static int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);

        try
        {
            listener.Start();
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }
}

