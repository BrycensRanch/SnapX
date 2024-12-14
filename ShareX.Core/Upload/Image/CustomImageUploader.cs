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
using ShareX.Core.Utils.Miscellaneous;

namespace ShareX.Core.Upload.Image;

public class CustomImageUploaderService : ImageUploaderService
{
    public override ImageDestination EnumValue => ImageDestination.CustomImageUploader;

    public override bool CheckConfig(UploadersConfig config)
    {
        return config.CustomUploadersList != null && config.CustomUploadersList.IsValidIndex(config.CustomImageUploaderSelected);
    }

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        var index = taskInfo.OverrideCustomUploader
            ? taskInfo.CustomUploaderIndex.BetweenOrDefault(0, config.CustomUploadersList.Count - 1)
            : config.CustomImageUploaderSelected;

        var customUploader = config.CustomUploadersList.ReturnIfValidIndex(index);

        if (customUploader == null)
        {
            return null;
        }

        return new CustomImageUploader(customUploader);
    }
}

public sealed class CustomImageUploader : ImageUploader
{
    private CustomUploaderItem uploader;

    public CustomImageUploader(CustomUploaderItem customUploaderItem)
    {
        uploader = customUploaderItem;
    }

    public override UploadResult Upload(Stream stream, string fileName)
    {
        var ur = new UploadResult();
        var input = new CustomUploaderInput(fileName, "");

        if (uploader.Body == CustomUploaderBody.MultipartFormData)
        {
            ur = SendRequestFile(uploader.GetRequestURL(input), stream, fileName, uploader.GetFileFormName(), uploader.GetArguments(input),
                uploader.GetHeaders(input), null, uploader.RequestMethod);
        }
        else if (uploader.Body == CustomUploaderBody.Binary)
        {
            ur.Response = SendRequest(uploader.RequestMethod, uploader.GetRequestURL(input), stream, MimeTypes.GetMimeTypeFromFileName(fileName),
                null, uploader.GetHeaders(input));
        }
        else
        {
            throw new Exception("Unsupported request format: " + uploader.Body);
        }

        uploader.TryParseResponse(ur, LastResponseInfo, Errors, input);

        return ur;
    }
}

