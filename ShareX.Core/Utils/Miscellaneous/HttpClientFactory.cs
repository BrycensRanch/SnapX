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

using System.Net.Http.Headers;

namespace ShareX.Core.Utils.Miscellaneous;
public static class HttpClientFactory
{
    // Using Lazy<T> to handle thread-safe initialization of the HttpClient
    private static Lazy<HttpClient> _lazyClient = new Lazy<HttpClient>(() =>
    {
        var clientHandler = new HttpClientHandler
        {
            Proxy = HelpersOptions.CurrentProxy.GetWebProxy()
        };

        var httpClient = new HttpClient(clientHandler);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ShareXResources.UserAgent);
        httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true
        };

        return httpClient;
    });

    public static HttpClient Create() => _lazyClient.Value;

    // Resets the HttpClient, disposing the current instance if it exists
    public static void Reset()
    {
        if (_lazyClient.IsValueCreated)
        {
            _lazyClient.Value.Dispose();
            _lazyClient = new Lazy<HttpClient>(() =>
            {
                var clientHandler = new HttpClientHandler
                {
                    Proxy = HelpersOptions.CurrentProxy.GetWebProxy()
                };

                var httpClient = new HttpClient(clientHandler);
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ShareXResources.UserAgent);
                httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true
                };

                return httpClient;
            });
        }
    }
}

