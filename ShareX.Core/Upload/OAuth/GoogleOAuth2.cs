
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Collections.Specialized;
using System.Text.Json;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Utils;

namespace ShareX.Core.Upload.OAuth;

public class GoogleOAuth2 : IOAuth2Loopback
{
    private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";

    public OAuth2Info AuthInfo { get; private set; }
    private Uploader GoogleUploader { get; set; }
    public string RedirectURI { get; set; }
    public string State { get; set; }
    public string Scope { get; set; }

    public GoogleOAuth2(OAuth2Info oauth, Uploader uploader)
    {
        AuthInfo = oauth;
        GoogleUploader = uploader;
    }

    public string GetAuthorizationURL()
    {
        var args = new Dictionary<string, string>
        {
            { "response_type", "code" },
            { "client_id", AuthInfo.Client_ID },
            { "redirect_uri", RedirectURI },
            { "state", State },
            { "scope", Scope }
        };

        return URLHelpers.CreateQueryString(AuthorizationEndpoint, args);
    }


    public bool GetAccessToken(string code)
    {
        var args = new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", AuthInfo.Client_ID },
            { "client_secret", AuthInfo.Client_Secret },
            { "redirect_uri", RedirectURI },
            { "grant_type", "authorization_code" }
        };

        var response = GoogleUploader.SendRequestURLEncoded(HttpMethod.Post, TokenEndpoint, args);

        if (string.IsNullOrEmpty(response)) return false;

        var token = JsonSerializer.Deserialize<OAuth2Token>(response);
        if (token?.access_token == null) return false;

        token.UpdateExpireDate();
        AuthInfo.Token = token;
        return true;
    }


    public bool RefreshAccessToken()
    {
        if (!OAuth2Info.CheckOAuth(AuthInfo) || string.IsNullOrEmpty(AuthInfo.Token.refresh_token))
            return false;

        var args = new Dictionary<string, string>
        {
            { "refresh_token", AuthInfo.Token.refresh_token },
            { "client_id", AuthInfo.Client_ID },
            { "client_secret", AuthInfo.Client_Secret },
            { "grant_type", "refresh_token" }
        };

        var response = GoogleUploader.SendRequestURLEncoded(HttpMethod.Post, TokenEndpoint, args);

        if (string.IsNullOrEmpty(response)) return false;

        var token = JsonSerializer.Deserialize<OAuth2Token>(response);

        if (token?.access_token == null) return false;

        token.UpdateExpireDate();

        AuthInfo.Token = token;

        return true;
    }


    public bool CheckAuthorization()
    {
        if (OAuth2Info.CheckOAuth(AuthInfo))
        {
            if (AuthInfo.Token.IsExpired && !RefreshAccessToken())
            {
                GoogleUploader.Errors.Add("Refresh access token failed.");
                return false;
            }
        }
        else
        {
            GoogleUploader.Errors.Add("Login is required.");
            return false;
        }

        return true;
    }

    public NameValueCollection GetAuthHeaders()
    {
        var headers = new NameValueCollection
        {
            { "Authorization", $"Bearer {AuthInfo.Token.access_token}" }
        };

        return headers;
    }

    public OAuthUserInfo GetUserInfo()
    {
        var response = GoogleUploader.SendRequest(HttpMethod.Get, UserInfoEndpoint, null, GetAuthHeaders());

        return !string.IsNullOrEmpty(response)
            ? JsonSerializer.Deserialize<OAuthUserInfo>(response)
            : null;
    }

}

