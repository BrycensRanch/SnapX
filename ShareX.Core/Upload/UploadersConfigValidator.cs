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

namespace ShareX.Core.Upload;

public static class UploadersConfigValidator
{
    public static bool Validate<T>(int index, UploadersConfig config)
    {
        var destination = (Enum)Enum.ToObject(typeof(T), index);

        return destination switch
        {
            ImageDestination imageDestination => Validate(imageDestination, config),
            TextDestination textDestination => Validate(textDestination, config),
            FileDestination fileDestination => Validate(fileDestination, config),
            UrlShortenerType urlShortenerType => Validate(urlShortenerType, config),
            URLSharingServices urlSharingServices => Validate(urlSharingServices, config),
            _ => true
        };
    }

    public static bool Validate(ImageDestination destination, UploadersConfig config) =>
        destination == ImageDestination.FileUploader ||
        UploaderFactory.ImageUploaderServices[destination].CheckConfig(config);

    public static bool Validate(TextDestination destination, UploadersConfig config) =>
        destination == TextDestination.FileUploader ||
        UploaderFactory.TextUploaderServices[destination].CheckConfig(config);


    public static bool Validate(FileDestination destination, UploadersConfig config) =>
        UploaderFactory.FileUploaderServices[destination].CheckConfig(config);

    public static bool Validate(UrlShortenerType destination, UploadersConfig config) =>
        UploaderFactory.URLShortenerServices[destination].CheckConfig(config);

    public static bool Validate(URLSharingServices destination, UploadersConfig config) =>
        UploaderFactory.URLSharingServices[destination].CheckConfig(config);

}

