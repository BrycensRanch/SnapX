
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShareX.Core.Utils;

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
        var directory = Path.GetDirectoryName(filePath);
        if (directory == null) return;
        Directory.CreateDirectory(directory);

        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        SerializeToStream(obj, fileStream, options);
    }

    public static T Deserialize<T>(TextReader textReader, JsonSerializerOptions options = null)
    {
        if (textReader == null) return default;

        var json = textReader.ReadToEnd();
        return JsonSerializer.Deserialize<T>(json, options);
    }

    public static T DeserializeFromString<T>(string json, JsonSerializerOptions options = null) =>
        !string.IsNullOrEmpty(json) ? JsonSerializer.Deserialize<T>(json, options) : default;

    public static T DeserializeFromStream<T>(Stream stream, JsonSerializerOptions options = null)
    {
        if (stream == null) return default;

        return JsonSerializer.Deserialize<T>(stream, options);
    }

    public static T DeserializeFromFile<T>(string filePath, JsonSerializerOptions options = null)
    {
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return default;

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return DeserializeFromStream<T>(fileStream, options);
    }

    public static bool QuickVerifyJsonFile(string filePath)
    {
        try
        {
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (fileStream.Length > 1 && fileStream.ReadByte() == (byte)'{')
                {
                    fileStream.Seek(-1, SeekOrigin.End);
                    return fileStream.ReadByte() == (byte)'}';
                }
            }
        }
        catch
        {
            // I acknowledge I am swallowing the error....
        }

        return false;
    }
}

