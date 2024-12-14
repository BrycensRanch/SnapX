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

using System.ComponentModel;
using System.Text.Json;
using System.Xml.Linq;
using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.OAuth;
using ShareX.Core.Upload.Utils;
using ShareX.Core.Utils.Extensions;
using ShareX.Core.Utils.Miscellaneous;

namespace ShareX.Core.Upload.Image;

public class FlickrImageUploaderService : ImageUploaderService
{
    public override ImageDestination EnumValue => ImageDestination.Flickr;
    public override bool CheckConfig(UploadersConfig config)
    {
        return OAuthInfo.CheckOAuth(config.FlickrOAuthInfo);
    }

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new FlickrUploader(config.FlickrOAuthInfo, config.FlickrSettings);
    }
}

public class FlickrUploader : ImageUploader, IOAuth
{
    public OAuthInfo AuthInfo { get; set; }
    public FlickrSettings Settings { get; set; } = new();

    public FlickrUploader(OAuthInfo oauth)
    {
        AuthInfo = oauth;
    }

    public FlickrUploader(OAuthInfo oauth, FlickrSettings settings)
    {
        AuthInfo = oauth;
        Settings = settings;
    }

    public string GetAuthorizationURL()
    {
        Dictionary<string, string> args = new Dictionary<string, string>();
        args.Add("oauth_callback", Links.Callback);

        string url = GetAuthorizationURL("https://www.flickr.com/services/oauth/request_token", "https://www.flickr.com/services/oauth/authorize", AuthInfo, args);

        return url + "&perms=write";
    }

    public bool GetAccessToken(string verificationCode = null)
    {
        AuthInfo.AuthVerifier = verificationCode;
        return GetAccessToken("https://www.flickr.com/services/oauth/access_token", AuthInfo);
    }

    public FlickrPhotosGetSizesResponse PhotosGetSizes(string photoid)
    {
        if (string.IsNullOrEmpty(photoid))
        {
            throw new ArgumentException("Photo ID cannot be null or empty.", nameof(photoid));
        }

        var args = new Dictionary<string, string>
        {
            { "nojsoncallback", "1" },
            { "format", "json" },
            { "method", "flickr.photos.getSizes" },
            { "photo_id", photoid }
        };

        string query = OAuthManager.GenerateQuery("https://api.flickr.com/services/rest", args, HttpMethod.Post, AuthInfo);
        string response = SendRequest(HttpMethod.Get, query);

        return string.IsNullOrEmpty(response)
            ? null
            : JsonSerializer.Deserialize<FlickrPhotosGetSizesResponse>(response);
    }


    public override UploadResult Upload(Stream stream, string fileName)
    {
        var url = "https://up.flickr.com/services/upload/";

        var args = new Dictionary<string, string>();

        // Add valid keys only if their values are non-null and non-empty
        if (!string.IsNullOrEmpty(Settings.Title)) args.Add("title", Settings.Title);
        if (!string.IsNullOrEmpty(Settings.Description)) args.Add("description", Settings.Description);
        if (!string.IsNullOrEmpty(Settings.Tags)) args.Add("tags", Settings.Tags);
        if (!string.IsNullOrEmpty(Settings.IsPublic)) args.Add("is_public", Settings.IsPublic);
        if (!string.IsNullOrEmpty(Settings.IsFriend)) args.Add("is_friend", Settings.IsFriend);
        if (!string.IsNullOrEmpty(Settings.IsFamily)) args.Add("is_family", Settings.IsFamily);
        if (!string.IsNullOrEmpty(Settings.SafetyLevel)) args.Add("safety_level", Settings.SafetyLevel);
        if (!string.IsNullOrEmpty(Settings.ContentType)) args.Add("content_type", Settings.ContentType);
        if (!string.IsNullOrEmpty(Settings.Hidden)) args.Add("hidden", Settings.Hidden);

        OAuthManager.GenerateQuery(url, args, HttpMethod.Post, AuthInfo, out Dictionary<string, string> parameters);

        var result = SendRequestFile(url, stream, fileName, "photo", parameters);

        if (!result.IsSuccess) return result;

        var xele = ParseResponse(result.Response, "photoid");

        if (xele == null) return result;

        string photoid = xele.Value;

        var photos = PhotosGetSizes(photoid);

        if (photos?.sizes?.size?.Length > 0)
        {
            var photo = photos.sizes.size.Last();
            result.URL = Settings.DirectLink ? photo.source : photo.url;
        }

        return result;
    }


    private XElement ParseResponse(string response, string field)
    {
        if (string.IsNullOrEmpty(response)) return null;

        try
        {
            var xdoc = XDocument.Parse(response);
            var rspElement = xdoc.Element("rsp");

            if (rspElement == null) return null;

            var status = rspElement.GetAttributeFirstValue("status", "stat");

            if (status == "ok")
            {
                return rspElement.Element(field);
            }

            if (status == "fail")
            {
                var err = rspElement.Element("err");
                if (err != null)
                {
                    var errorMsg = err.GetAttributeValue("msg");
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        Errors.Add(errorMsg);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Errors.Add("Failed to parse response: " + ex.Message);
        }

        return null;
    }
}

public class FlickrSettings
{
    public bool DirectLink { get; set; } = true;

    [Description("The title of the photo.")]
    public string Title { get; set; }

    [Description("A description of the photo. May contain some limited HTML.")]
    public string Description { get; set; }

    [Description("A space-seperated list of tags to apply to the photo.")]
    public string Tags { get; set; }

    [Description("Set to 0 for no, 1 for yes. Specifies who can view the photo.")]
    public string IsPublic { get; set; }

    [Description("Set to 0 for no, 1 for yes. Specifies who can view the photo.")]
    public string IsFriend { get; set; }

    [Description("Set to 0 for no, 1 for yes. Specifies who can view the photo.")]
    public string IsFamily { get; set; }

    [Description("Set to 1 for Safe, 2 for Moderate, or 3 for Restricted.")]
    public string SafetyLevel { get; set; }

    [Description("Set to 1 for Photo, 2 for Screenshot, or 3 for Other.")]
    public string ContentType { get; set; }

    [Description("Set to 1 to keep the photo in global search results, 2 to hide from public searches.")]
    public string Hidden { get; set; }
}

public class FlickrPhotosGetSizesResponse
{
    public FlickrPhotosGetSizesSizes sizes { get; set; }
    public string stat { get; set; }
}

public class FlickrPhotosGetSizesSizes
{
    public int canblog { get; set; }
    public bool canprint { get; set; }
    public int candownload { get; set; }
    public FlickrPhotosGetSizesSize[] size { get; set; }
}

public class FlickrPhotosGetSizesSize
{
    public string label { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public string source { get; set; }
    public string url { get; set; }
    public string media { get; set; }
}

