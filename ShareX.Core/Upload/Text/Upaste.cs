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

using System.Text.Json;
using ShareX.Core.Upload.BaseServices;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Upload.Utils;

namespace ShareX.Core.Upload.Text;

public class UpasteTextUploaderService : TextUploaderService
{
    public override TextDestination EnumValue => TextDestination.Upaste;
    public override bool CheckConfig(UploadersConfig config) => true;

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new Upaste(config.UpasteUserKey)
        {
            IsPublic = config.UpasteIsPublic
        };
    }
}

public sealed class Upaste : TextUploader
{
    private const string APIURL = "https://upaste.me/api";

    public string UserKey { get; private set; }
    public bool IsPublic { get; set; }

    public Upaste(string userKey)
    {
        UserKey = userKey;
    }

    public override UploadResult UploadText(string text, string fileName)
    {
        var ur = new UploadResult();

        if (string.IsNullOrEmpty(text))
            return ur;

        var arguments = new Dictionary<string, string>
        {
            { "paste", text },
            { "privacy", IsPublic ? "0" : "1" },
            { "expire", "0" },
            { "json", "true" }
        };

        if (!string.IsNullOrEmpty(UserKey))
        {
            arguments.Add("api_key", UserKey);
        }

        ur.Response = SendRequestMultiPart(APIURL, arguments);

        if (string.IsNullOrEmpty(ur.Response))
            return ur;

        // Deserialize response
        var response = JsonSerializer.Deserialize<UpasteResponse>(ur.Response);

        if (response?.status?.Equals("success", StringComparison.OrdinalIgnoreCase) == true)
        {
            ur.URL = response.paste.link;
        }
        else
        {
            Errors.Add(response?.error);
        }

        return ur;
    }


    public class UpastePaste
    {
        public string id { get; set; }
        public string link { get; set; }
        public string raw { get; set; }
        public string download { get; set; }
    }

    public class UpasteResponse
    {
        public UpastePaste paste { get; set; }
        public int errorcode { get; set; }
        public string error { get; set; }
        public string status { get; set; }
    }
}

