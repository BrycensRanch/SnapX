
// SPDX-License-Identifier: GPL-3.0-or-later


using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SnapX.Core.Utils;

namespace SnapX.Core.History;
[JsonSerializable(typeof(HistoryItem))]

internal partial class HistoryContext : JsonSerializerContext
{
}

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
            return [];

        lock (thisLock)
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);

            if (string.IsNullOrEmpty(json))
                return [];

            json = "[" + json + "]";
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                TypeInfoResolver = HistoryContext.Default
            };

            return JsonSerializer.Deserialize<List<HistoryItem>>(json, options) ?? [];
        }
    }

    [RequiresDynamicCode("Uploader")]
    [RequiresUnreferencedCode("Uploader")]
    protected override bool Append(string filePath, IEnumerable<HistoryItem> historyItems)
    {
        if (string.IsNullOrEmpty(filePath)) return false;
        lock (thisLock)
        {
            FileHelpers.CreateDirectoryFromFilePath(filePath);

            using var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096,
                FileOptions.WriteThrough);
            using var streamWriter = new StreamWriter(fileStream);
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                TypeInfoResolver = HistoryContext.Default,
                WriteIndented = false
            };

            bool isFirstObject = fileStream.Length == 0;

            foreach (var historyItem in historyItems)
            {
                if (!isFirstObject)
                    streamWriter.Write(",\r\n");

                string json = JsonSerializer.Serialize(historyItem, options);
                streamWriter.Write(json);

                isFirstObject = false;
            }

            Backup(filePath);
            return true;
        }
    }
}

