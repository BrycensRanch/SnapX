
// SPDX-License-Identifier: GPL-3.0-or-later


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

