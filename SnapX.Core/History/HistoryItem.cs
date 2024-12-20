
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SnapX.Core.History;
public class HistoryItem
{
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public DateTime DateTime { get; set; }
    public string Type { get; set; }
    public string Host { get; set; }
    public string URL { get; set; }
    public string ThumbnailURL { get; set; }
    public string DeletionURL { get; set; }
    public string ShortenedURL { get; set; }

    [Browsable(false)]
    public Dictionary<string, string> Tags { get; set; }

    [JsonIgnore, DisplayName("Tags[WindowTitle]")]
    public string TagsWindowTitle
    {
        get
        {
            if (Tags != null && Tags.TryGetValue("WindowTitle", out string value))
            {
                return value;
            }

            return null;
        }
    }

    [JsonIgnore, DisplayName("Tags[ProcessName]")]
    public string TagsProcessName
    {
        get
        {
            if (Tags != null && Tags.TryGetValue("ProcessName", out string value))
            {
                return value;
            }

            return null;
        }
    }
}

