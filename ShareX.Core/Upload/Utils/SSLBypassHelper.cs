
// SPDX-License-Identifier: GPL-3.0-or-later


using ShareX.Core.Utils.Miscellaneous;

namespace ShareX.Core.Upload.Utils;

public class SSLBypassHelper : IDisposable
{
    private readonly HttpClientHandler _httpClientHandler;
    private readonly HttpClient _httpClient;

    public SSLBypassHelper()
    {
        _httpClientHandler = new HttpClientHandler
        {
            // Allow all SSL certificates
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        _httpClient = HttpClientFactory.Create();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _httpClientHandler.Dispose();
    }
}

