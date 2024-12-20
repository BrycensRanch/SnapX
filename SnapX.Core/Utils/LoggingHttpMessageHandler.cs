using Serilog;

namespace SnapX.Core.Utils;

public class LoggingHttpMessageHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public LoggingHttpMessageHandler(HttpMessageHandler innerHandler, ILogger logger)
        : base(innerHandler)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.Information("Sending HTTP Request: {Method} {Uri} {@Headers}",
            request.Method, request.RequestUri, request.Headers);

        var response = await base.SendAsync(request, cancellationToken);

        _logger.Information("Received HTTP Response: {StatusCode} for {Method} {Uri}",
            response.StatusCode, request.Method, request.RequestUri);

        _logger.Debug("Response Headers: {@Headers}", response.Headers);

        // Be careful, some response bodies are huge...
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _logger.Debug("Response Body: {Content}", content);
        }

        return response;
    }
}
