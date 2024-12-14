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
using ShareX.Core.Utils;

namespace ShareX.Core.Upload.Text;

public class HastebinTextUploaderService : TextUploaderService
{
    public override TextDestination EnumValue => TextDestination.Hastebin;
    public override bool CheckConfig(UploadersConfig config) => true;

    public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
    {
        return new Hastebin()
        {
            CustomDomain = config.HastebinCustomDomain,
            SyntaxHighlighting = config.HastebinSyntaxHighlighting,
            UseFileExtension = config.HastebinUseFileExtension
        };
    }
}

public sealed class Hastebin : TextUploader
{
    public string CustomDomain { get; set; }
    public string SyntaxHighlighting { get; set; }
    public bool UseFileExtension { get; set; }

    public override UploadResult UploadText(string text, string fileName)
    {
        var ur = new UploadResult();
        if (string.IsNullOrEmpty(text)) return ur;
        var domain = !string.IsNullOrEmpty(CustomDomain) ? CustomDomain : "https://hastebin.com";

        ur.Response = SendRequest(HttpMethod.Post, URLHelpers.CombineURL(domain, "documents"), text);
        if (string.IsNullOrEmpty(ur.Response)) return ur;

        var response = JsonSerializer.Deserialize<HastebinResponse>(ur.Response);
        if (response == null || string.IsNullOrEmpty(response.Key)) return ur;

        var url = URLHelpers.CombineURL(domain, response.Key);

        var syntaxHighlighting = SyntaxHighlighting;

        if (UseFileExtension)
        {
            var ext = FileHelpers.GetFileNameExtension(fileName);

            if (!string.IsNullOrEmpty(ext) && !ext.Equals("txt", StringComparison.OrdinalIgnoreCase))
            {
                syntaxHighlighting = ext.ToLowerInvariant();
            }
        }

        if (!string.IsNullOrEmpty(syntaxHighlighting))
        {
            url += "." + syntaxHighlighting;
        }

        ur.URL = url;


        return ur;
    }

    private class HastebinResponse
    {
        public string Key { get; set; }
    }
}

