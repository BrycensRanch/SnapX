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


using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Custom;
using ShareX.Core.Upload.Utils;
using ShareX.Core.Utils.Extensions;

namespace ShareX.Core.Upload.SharingServices;

public class CustomURLSharingService : URLSharingService
{
    public override URLSharingServices EnumValue => URLSharingServices.CustomURLSharingService;

    public override bool CheckConfig(UploadersConfig config)
    {
        return config.CustomUploadersList != null && config.CustomUploadersList.IsValidIndex(config.CustomURLSharingServiceSelected);
    }

    public override URLSharer CreateSharer(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        int index = taskInfo.OverrideCustomUploader
            ? taskInfo.CustomUploaderIndex.BetweenOrDefault(0, config.CustomUploadersList.Count - 1)
            : config.CustomURLSharingServiceSelected;

        var customUploader = config.CustomUploadersList.ReturnIfValidIndex(index);

        return customUploader != null ? new CustomURLSharer(customUploader) : null;
    }
}

public sealed class CustomURLSharer : URLSharer
{
    private CustomUploaderItem uploader;

    public CustomURLSharer(CustomUploaderItem customUploaderItem)
    {
        uploader = customUploaderItem;
    }

    public override UploadResult ShareURL(string url)
    {
        var result = new UploadResult { URL = url, IsURLExpected = false };
        var input = new CustomUploaderInput("", url);

        var response = uploader.Body switch
        {
            CustomUploaderBody.None => SendRequest(uploader.RequestMethod, uploader.GetRequestURL(input), null, uploader.GetHeaders(input)),
            CustomUploaderBody.MultipartFormData => SendRequestMultiPart(uploader.GetRequestURL(input), uploader.GetArguments(input), uploader.GetHeaders(input), null, uploader.RequestMethod),
            CustomUploaderBody.FormURLEncoded => SendRequestURLEncoded(uploader.RequestMethod, uploader.GetRequestURL(input), uploader.GetArguments(input), uploader.GetHeaders(input)),
            CustomUploaderBody.JSON or CustomUploaderBody.XML => SendRequest(uploader.RequestMethod, uploader.GetRequestURL(input), uploader.GetData(input), uploader.GetContentType(), null, uploader.GetHeaders(input)),
            _ => throw new Exception("Unsupported request format: " + uploader.Body)
        };

        result.Response = response;
        return result;
    }
}

