﻿#region License Information (GPL v3)

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

