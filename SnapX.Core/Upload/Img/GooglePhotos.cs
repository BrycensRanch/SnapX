
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Collections.Specialized;
using System.Text.Json;
using SnapX.Core.Upload.BaseServices;
using SnapX.Core.Upload.BaseUploaders;
using SnapX.Core.Upload.OAuth;
using SnapX.Core.Upload.Utils;

namespace SnapX.Core.Upload.Img;

public class GooglePhotosImageUploaderService : ImageUploaderService
{
    public override ImageDestination EnumValue => ImageDestination.Picasa;

    public override bool CheckConfig(UploadersConfig config)
    {
        return OAuth2Info.CheckOAuth(config.GooglePhotosOAuth2Info);
    }

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new GooglePhotos(config.GooglePhotosOAuth2Info)
        {
            AlbumID = config.GooglePhotosAlbumID,
            IsPublic = config.GooglePhotosIsPublic
        };
    }
}

public sealed class GooglePhotos : ImageUploader, IOAuth2
{
    public GoogleOAuth2 OAuth2 { get; private set; }
    public OAuth2Info AuthInfo => OAuth2.AuthInfo;
    public string AlbumID { get; set; }
    public bool IsPublic { get; set; }

    public GooglePhotos(OAuth2Info oauth)
    {
        OAuth2 = new GoogleOAuth2(oauth, this)
        {
            Scope = "https://www.googleapis.com/auth/photoslibrary https://www.googleapis.com/auth/photoslibrary.sharing https://www.googleapis.com/auth/userinfo.profile"
        };
    }

    public bool RefreshAccessToken()
    {
        return OAuth2.RefreshAccessToken();
    }

    public bool CheckAuthorization()
    {
        return OAuth2.CheckAuthorization();
    }

    public string GetAuthorizationURL()
    {
        return OAuth2.GetAuthorizationURL();
    }

    public bool GetAccessToken(string code)
    {
        return OAuth2.GetAccessToken(code);
    }

    public OAuthUserInfo GetUserInfo()
    {
        return OAuth2.GetUserInfo();
    }

    public GooglePhotosAlbum CreateAlbum(string albumName)
    {
        var newItemAlbum = new GooglePhotosNewAlbum
        {
            album = new GooglePhotosAlbum
            {
                title = albumName
            }
        };

        var newItemAlbumArgs = new Dictionary<string, string>
        {
            { "fields", "id" }
        };

        var serializedNewItemAlbum = JsonSerializer.Serialize(newItemAlbum);
        var serializedNewItemAlbumResponse = SendRequest(HttpMethod.Post, "https://photoslibrary.googleapis.com/v1/albums", serializedNewItemAlbum, RequestHelpers.ContentTypeJSON, newItemAlbumArgs, OAuth2.GetAuthHeaders());
        var newItemAlbumResponse = JsonSerializer.Deserialize<GooglePhotosAlbum>(serializedNewItemAlbumResponse);

        return newItemAlbumResponse;
    }

    public List<GooglePhotosAlbumInfo> GetAlbumList()
    {
        if (!CheckAuthorization()) return null;

        var albumList = new List<GooglePhotosAlbumInfo>();

        var albumListArgs = new Dictionary<string, string>
        {
            { "excludeNonAppCreatedData", "true" },
            { "fields", "albums(id,title,shareInfo),nextPageToken" }
        };

        var pageToken = "";

        do
        {
            albumListArgs["pageToken"] = pageToken;
            var response = SendRequest(HttpMethod.Get, "https://photoslibrary.googleapis.com/v1/albums", albumListArgs, OAuth2.GetAuthHeaders());
            pageToken = "";

            if (!string.IsNullOrEmpty(response))
            {
                var albumsResponse = JsonSerializer.Deserialize<GooglePhotosAlbums>(response);

                if (albumsResponse.albums != null)
                {
                    foreach (GooglePhotosAlbum album in albumsResponse.albums)
                    {
                        var AlbumInfo = new GooglePhotosAlbumInfo
                        {
                            ID = album.id,
                            Name = album.title
                        };

                        if (album.shareInfo == null)
                        {
                            albumList.Add(AlbumInfo);
                        }
                    }
                }
                pageToken = albumsResponse.nextPageToken;
            }
        }
        while (!string.IsNullOrEmpty(pageToken));

        return albumList;
    }

    public override UploadResult Upload(Stream stream, string fileName)
    {
        if (!CheckAuthorization()) return null;

        var result = new UploadResult();

        if (IsPublic)
        {
            AlbumID = CreateAlbum(fileName).id;

            var albumOptionsResponseArgs = new Dictionary<string, string>
            {
                { "fields", "shareInfo/shareableUrl" }
            };

            var albumOptions = new GooglePhotosAlbumOptions();

            var serializedAlbumOptions = JsonSerializer.Serialize(albumOptions);
            var serializedAlbumOptionsResponse = SendRequest(HttpMethod.Post, $"https://photoslibrary.googleapis.com/v1/albums/{AlbumID}:share", serializedAlbumOptions, RequestHelpers.ContentTypeJSON, albumOptionsResponseArgs, OAuth2.GetAuthHeaders());
            var albumOptionsResponse = JsonSerializer.Deserialize<GooglePhotosAlbumOptionsResponse>(serializedAlbumOptionsResponse);

            result.URL = albumOptionsResponse.shareInfo.shareableUrl;
        }

        var uploadTokenHeaders = new NameValueCollection
        {
            { "X-Goog-Upload-File-Name", fileName },
            { "X-Goog-Upload-Protocol", "raw" },
            { "Authorization", OAuth2.GetAuthHeaders()["Authorization"] }
        };

        var uploadToken = SendRequest(HttpMethod.Post, "https://photoslibrary.googleapis.com/v1/uploads", stream, RequestHelpers.ContentTypeOctetStream, null, uploadTokenHeaders);

        var newMediaItemRequest = new GooglePhotosNewMediaItemRequest
        {
            albumId = AlbumID,
            newMediaItems = new[]
            {
                new  GooglePhotosNewMediaItem
                {
                    simpleMediaItem = new GooglePhotosSimpleMediaItem
                    {
                        uploadToken = uploadToken
                    }
                }
            }
        };

        var newMediaItemRequestArgs = new Dictionary<string, string>
        {
            { "fields", "newMediaItemResults(mediaItem/productUrl)" }
        };

        var serializedNewMediaItemRequest = JsonSerializer.Serialize(newMediaItemRequest);

        result.Response = SendRequest(HttpMethod.Post, "https://photoslibrary.googleapis.com/v1/mediaItems:batchCreate", serializedNewMediaItemRequest, RequestHelpers.ContentTypeJSON, newMediaItemRequestArgs, OAuth2.GetAuthHeaders());

        var newMediaItemResult = JsonSerializer.Deserialize<GooglePhotosNewMediaItemResults>(result.Response);

        if (!IsPublic)
        {
            result.URL = newMediaItemResult.newMediaItemResults[0].mediaItem.productUrl;
        }

        return result;
    }
}

public class GooglePhotosAlbumInfo
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Summary { get; set; }
}

public class GooglePhotosAlbums
{
    public GooglePhotosAlbum[] albums { get; set; }
    public string nextPageToken { get; set; }
}

public class GooglePhotosAlbum
{
    public string id { get; set; }
    public string title { get; set; }
    public string productUrl { get; set; }
    public string coverPhotoBaseUrl { get; set; }
    public string coverPhotoMediaItemId { get; set; }
    public string isWriteable { get; set; }
    public GooglePhotosShareInfo shareInfo { get; set; }
    public string mediaItemsCount { get; set; }
}

public class GooglePhotosNewMediaItemRequest
{
    public string albumId { get; set; }
    public GooglePhotosNewMediaItem[] newMediaItems { get; set; }
}

public class GooglePhotosNewMediaItem
{
    public string description { get; set; }
    public GooglePhotosSimpleMediaItem simpleMediaItem { get; set; }
}

public class GooglePhotosSimpleMediaItem
{
    public string uploadToken { get; set; }
}

public class GooglePhotosNewMediaItemResults
{
    public GooglePhotosNewMediaItemResult[] newMediaItemResults { get; set; }
}

public class GooglePhotosNewMediaItemResult
{
    public string uploadToken { get; set; }
    public GooglePhotosStatus status { get; set; }
    public GooglePhotosMediaItem mediaItem { get; set; }
}

public class GooglePhotosStatus
{
    public string message { get; set; }
    public int code { get; set; }
}

public class GooglePhotosMediaItem
{
    public string id { get; set; }
    public string productUrl { get; set; }
    public string description { get; set; }
    public string baseUrl { get; set; }
    public GooglePhotosMediaMetaData mediaMetadata { get; set; }
}

public class GooglePhotosMediaMetaData
{
    public string width { get; set; }
    public string height { get; set; }
    public string creationTime { get; set; }
    public GooglePhotosPhoto photo { get; set; }
}

public class GooglePhotosPhoto
{
}

public class GooglePhotosNewAlbum
{
    public GooglePhotosAlbum album { get; set; }
}

public class GooglePhotosAlbumOptions
{
    public GooglePhotosSharedAlbumOptions sharedAlbumOptions { get; set; }
}

public class GooglePhotosSharedAlbumOptions
{
    public string isCollaborative { get; set; }
    public string isCommentable { get; set; }
}

public class GooglePhotosAlbumOptionsResponse
{
    public GooglePhotosShareInfo shareInfo { get; set; }
}

public class GooglePhotosShareInfo
{
    public GooglePhotosSharedAlbumOptions sharedAlbumOptions { get; set; }
    public string shareableUrl { get; set; }
    public string shareToken { get; set; }
    public string isJoined { get; set; }
}

