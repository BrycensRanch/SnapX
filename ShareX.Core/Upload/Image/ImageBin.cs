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

using System.Text.RegularExpressions;
using ShareX.Core.Upload.BaseUploaders;

namespace ShareX.Core.Upload.Image;

public sealed class ImageBin : ImageUploader
{
    public override UploadResult Upload(Stream stream, string fileName)
    {
        var arguments = new Dictionary<string, string>
        {
            { "t", "file" },
            { "name", "ShareX" },
            { "tags", "ShareX" },
            { "description", "test" },
            { "adult", "t" },
            { "sfile", "Upload" },
            { "url", "" }
        };

        var result = SendRequestFile("https://imagebin.ca/upload.php", stream, fileName, "f", arguments);

        if (!result.IsSuccess)
        {
            return result;
        }

        var match = Regex.Match(result.Response, @"(?<=ca/view/).+(?=\.html'>)");
        if (!match.Success) return result;

        var imageUrl = $"https://imagebin.ca/img/{match.Value}{Path.GetExtension(fileName)}";
        result.URL = imageUrl;

        return result;
    }
}

