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
using System.Xml.Linq;
using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.OAuth;
using ShareX.Core.Upload.Utils;
using ShareX.Core.Utils.Extensions;

namespace ShareX.Core.Upload.Image;

public class PhotobucketImageUploaderService : ImageUploaderService
{
    public override ImageDestination EnumValue => ImageDestination.Photobucket;

    public override bool CheckConfig(UploadersConfig config)
    {
        return config.PhotobucketAccountInfo != null && OAuthInfo.CheckOAuth(config.PhotobucketOAuthInfo);
    }

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new Photobucket(config.PhotobucketOAuthInfo, config.PhotobucketAccountInfo);
    }
}

public sealed class Photobucket : ImageUploader, IOAuth
{
    private const string URLRequestToken = "http://api.photobucket.com/login/request";
    private const string URLAuthorize = "http://photobucket.com/apilogin/login";
    private const string URLAccessToken = "http://api.photobucket.com/login/access";

    public OAuthInfo AuthInfo { get; set; }
    public PhotobucketAccountInfo AccountInfo { get; set; }

    public Photobucket(OAuthInfo oauth)
    {
        AuthInfo = oauth;
        AccountInfo = new PhotobucketAccountInfo();
    }

    public Photobucket(OAuthInfo oauth, PhotobucketAccountInfo accountInfo)
    {
        AuthInfo = oauth;
        AccountInfo = accountInfo;
    }

    public string GetAuthorizationURL()
    {
        return GetAuthorizationURL(URLRequestToken, URLAuthorize, AuthInfo, null, HttpMethod.Post);
    }

    public bool GetAccessToken(string verificationCode)
    {
        AuthInfo.AuthVerifier = verificationCode;

        var nv = GetAccessTokenEx(URLAccessToken, AuthInfo, HttpMethod.Post);

        if (nv != null)
        {
            AccountInfo.Subdomain = nv["subdomain"];
            AccountInfo.AlbumID = nv["username"];
            return !string.IsNullOrEmpty(AccountInfo.Subdomain);
        }

        return false;
    }

    public PhotobucketAccountInfo GetAccountInfo()
    {
        return AccountInfo;
    }

    public override UploadResult Upload(Stream stream, string fileName)
    {
        return UploadMedia(stream, fileName, AccountInfo.ActiveAlbumPath);
    }

    public UploadResult UploadMedia(Stream stream, string fileName, string albumID)
    {
        var args = new Dictionary<string, string>
        {
            { "id", albumID }, // Album identifier.
            { "type", "image" } // Media type. Options are image, video, or base64.
        };

        var url = "https://api.photobucket.com/album/!/upload";
        var query = OAuthManager.GenerateQuery(url, args, HttpMethod.Post, AuthInfo);
        query = FixURL(query);

        var result = SendRequestFile(query, stream, fileName, "uploadfile");

        if (!result.IsSuccess) return result;

        var xd = XDocument.Parse(result.Response);
        var xe = xd.GetNode("response/content");

        if (xe == null) return result;

        result.URL = xe.GetElementValue("url");
        result.ThumbnailURL = xe.GetElementValue("thumb");

        return result;
    }

    public bool CreateAlbum(string albumID, string albumName)
    {
        var args = new Dictionary<string, string>
        {
            { "id", albumID }, // Album identifier.
            { "name", albumName } // Name of result. Must be between 2 and 50 characters.
        };

        var url = "http://api.photobucket.com/album/!";
        var query = OAuthManager.GenerateQuery(url, args, HttpMethod.Post, AuthInfo);
        query = FixURL(query);

        var response = SendRequestMultiPart(query, args);

        if (string.IsNullOrEmpty(response)) return false;

        var xd = XDocument.Parse(response);
        var xe = xd.GetNode("response");

        if (xe == null) return false;

        string status = xe.GetElementValue("status");

        return status == "OK";
    }

    private string FixURL(string url) => url.Replace("api.photobucket.com", AccountInfo.Subdomain);

}

public class PhotobucketAccountInfo
{
    public string Subdomain { get; set; }

    public string AlbumID { get; set; }

    public List<string> AlbumList = new List<string>();
    public int ActiveAlbumID = 0;

    public string ActiveAlbumPath
    {
        get
        {
            return AlbumList[ActiveAlbumID];
        }
    }
}

