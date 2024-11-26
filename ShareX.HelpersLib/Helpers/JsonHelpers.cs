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

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShareX.HelpersLib
{
    public static class JsonHelpers
    {
        public static void Serialize<T>(T obj, TextWriter textWriter, JsonSerializerOptions options = null)
        {
            if (textWriter == null) return;
            using var memoryStream = new MemoryStream();
            JsonSerializer.Serialize(memoryStream, obj, options);
            // Convert to string and write to TextWriter
            textWriter.Write(Encoding.UTF8.GetString(memoryStream.ToArray()));
        }

    public static string SerializeToString<T>(T obj, JsonSerializerOptions options = null)
    {
        options ??= new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = true
        };

        return JsonSerializer.Serialize(obj, options);
    }

    public static void SerializeToStream<T>(T obj, Stream stream, JsonSerializerOptions options = null)
    {
        if (stream == null) return;

        options ??= new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = true
        };

        JsonSerializer.Serialize(stream, obj, options);
    }

    public static MemoryStream SerializeToMemoryStream<T>(T obj, JsonSerializerOptions options = null)
    {
        var memoryStream = new MemoryStream();
        SerializeToStream(obj, memoryStream, options);
        return memoryStream;
    }

    public static void SerializeToFile<T>(T obj, string filePath, JsonSerializerOptions options = null)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
            SerializeToStream(obj, fileStream, options);
        }
    }

    public static T Deserialize<T>(TextReader textReader, JsonSerializerOptions options = null)
    {
        if (textReader == null) return default;

        var json = textReader.ReadToEnd();
        return JsonSerializer.Deserialize<T>(json, options);
    }

    public static T DeserializeFromString<T>(string json, JsonSerializerOptions options = null)
    {
        if (string.IsNullOrEmpty(json)) return default;

        return JsonSerializer.Deserialize<T>(json, options);
    }

    public static T DeserializeFromStream<T>(Stream stream, JsonSerializerOptions options = null)
    {
        if (stream == null) return default;

        return JsonSerializer.Deserialize<T>(stream, options);
    }

    public static T DeserializeFromFile<T>(string filePath, JsonSerializerOptions options = null)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return default;

        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            return DeserializeFromStream<T>(fileStream, options);
        }
    }

        public static bool QuickVerifyJsonFile(string filePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        if (fileStream.Length > 1 && fileStream.ReadByte() == (byte)'{')
                        {
                            fileStream.Seek(-1, SeekOrigin.End);
                            return fileStream.ReadByte() == (byte)'}';
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
