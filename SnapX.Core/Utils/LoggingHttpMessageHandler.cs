using System.Net.Security;
using System.Reflection;
using System.Text;
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
        try
        {
            _logger.Information("Sending HTTP Request: {Method} {Uri} {@Headers}",
                request.Method, request.RequestUri, request.Headers);
            var response = await base.SendAsync(request, cancellationToken);

            _logger.Information("Received HTTP Response: {StatusCode} for {Method} {Uri} (HTTP {Version})",
                response.StatusCode, request.Method, request.RequestUri, response.Version);

            _logger.Debug("Response Headers: {@Headers}", response.Headers);

            // Be careful, some response bodies are huge...
            var content = await response.Content.ReadAsStringAsync();
            var responseBodySizeBytes = Encoding.UTF8.GetByteCount(content);
            var responseBodySizeMiB = responseBodySizeBytes / (1024.0 * 1024.0);
            _logger.Debug("Response Body ({Size} MiB): {Content}", responseBodySizeMiB, content);
            return response;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ex.Message);
            return new HttpResponseMessage();
        }
    }
    static object GetProperty(object obj, string propertyName)
        => GetMemberInfo(obj,
                type => type.GetProperty(propertyName, bindingFlags)!)
            .GetValue(obj)!;

    static object GetField(object obj, string fieldName)
        => GetMemberInfo(obj,
                type => type.GetField(fieldName, bindingFlags)!)
            .GetValue(obj)!;

    static TMember GetMemberInfo<TMember>(object obj, Func<Type, TMember> getMemberInfo)
        where TMember : MemberInfo
        => getMemberInfo(obj.GetType());

    static BindingFlags bindingFlags =
        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
}
