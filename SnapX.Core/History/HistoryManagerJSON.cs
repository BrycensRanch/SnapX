
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using SnapX.Core.Utils;

namespace SnapX.Core.History;
public class HistoryManagerJSON : HistoryManager
{
    private static readonly object thisLock = new object();

    public HistoryManagerJSON(string filePath) : base(filePath)
    {
    }

    [RequiresDynamicCode("Uploader")]
    [RequiresUnreferencedCode("Uploader")]
    protected override List<HistoryItem> Load(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            return new List<HistoryItem>();

        lock (thisLock)
        {
            string json = System.IO.File.ReadAllText(filePath, Encoding.UTF8);

            if (string.IsNullOrEmpty(json))
                return new List<HistoryItem>();

            // Wrap the json in an array format (since the original code expected a JSON array)
            json = "[" + json + "]";

            // Deserialize the JSON string into a List of HistoryItem objects
            return JsonSerializer.Deserialize<List<HistoryItem>>(json) ?? new List<HistoryItem>();
        }
    }

    [RequiresDynamicCode("Uploader")]
    [RequiresUnreferencedCode("Uploader")]
    protected override bool Append(string filePath, IEnumerable<HistoryItem> historyItems)
    {
        if (string.IsNullOrEmpty(filePath)) return false;

        // Ensure the directory exists
        FileHelpers.CreateDirectoryFromFilePath(filePath);

        // Open the file for appending data
        using var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096,
            FileOptions.WriteThrough);
        using var streamWriter = new StreamWriter(fileStream);
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = false
        };

        // Check if this is the first object being written
        bool isFirstObject = fileStream.Length == 0;

        foreach (var historyItem in historyItems)
        {
            if (!isFirstObject)
                streamWriter.Write(",\r\n");

            // Serialize the current HistoryItem and write it to the stream
            string json = JsonSerializer.Serialize(historyItem, options);
            streamWriter.Write(json);

            isFirstObject = false;  // Ensure subsequent objects are properly comma-separated
        }

        // Backup after appending
        Backup(filePath);
        return true;
    }
}

