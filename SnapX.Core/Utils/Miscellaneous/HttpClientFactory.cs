
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Net.Http.Headers;

namespace SnapX.Core.Utils.Miscellaneous;
public static class HttpClientFactory
{
    // Using Lazy<T> to handle thread-safe initialization of the HttpClient
    private static Lazy<HttpClient> _lazyClient = new(() =>
    {
        var clientHandler = new HttpClientHandler
        {
            Proxy = HelpersOptions.CurrentProxy.GetWebProxy()
        };

        HttpMessageHandler handler = clientHandler;

#if DEBUG
        // Only for DEBUG. Do not enable in production. Or you'll be fired.
        var loggingHandler = new LoggingHttpMessageHandler(clientHandler, DebugHelper.Logger);
        handler = loggingHandler;
#endif
        var httpClient = new HttpClient(handler);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(SnapXResources.UserAgent);
        httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true
        };

        return httpClient;
    });


    public static HttpClient Get() => _lazyClient.Value;
}

