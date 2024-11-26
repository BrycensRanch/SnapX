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

using ShareX.HelpersLib;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ShareX.HistoryLib
{
    public class HistoryManagerJSON : HistoryManager
    {
        private static readonly object thisLock = new object();

        public HistoryManagerJSON(string filePath) : base(filePath)
        {
        }

        protected override List<HistoryItem> Load(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return new List<HistoryItem>();

            lock (thisLock)
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);

                if (string.IsNullOrEmpty(json))
                    return new List<HistoryItem>();

                // Wrap the json in an array format (since the original code expected a JSON array)
                json = "[" + json + "]";

                // Deserialize the JSON string into a List of HistoryItem objects
                return JsonSerializer.Deserialize<List<HistoryItem>>(json) ?? new List<HistoryItem>();
            }
        }

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
}
