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

namespace ShareX.Core.Indexer
{
    public class IndexerJson : Indexer
    {
        private Utf8JsonWriter jsonWriter;

        private string Index(string folderPath)
        {
            var folderInfo = new FolderInfo(folderPath);
            folderInfo.Update();

            var sbContent = new StringBuilder();

            // Use MemoryStream and Utf8JsonWriter for JSON writing
            using var memoryStream = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(memoryStream, new JsonWriterOptions { Indented = true });

            jsonWriter.WriteStartObject();
            IndexFolder(folderInfo);
            jsonWriter.WriteEndObject();

            // Convert the memory stream to a string
            sbContent.Append(Encoding.UTF8.GetString(memoryStream.ToArray()));

            return sbContent.ToString();
        }

        protected override void IndexFolder(FolderInfo dir, int level = 0)
        {
            IndexFolderSimple(dir);
        }

        private void IndexFolderSimple(FolderInfo dir)
        {
            jsonWriter.WritePropertyName(dir.FolderName);
            jsonWriter.WriteStartArray();

            foreach (FolderInfo subdir in dir.Folders)
            {
                jsonWriter.WriteStartObject();
                IndexFolder(subdir);
                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
        }

        private void IndexFolderParseable(FolderInfo dir)
        {
            jsonWriter.WritePropertyName("Name");

            if (dir.Folders.Count > 0)
            {
                jsonWriter.WritePropertyName("Folders");
                jsonWriter.WriteStartArray();

                foreach (FolderInfo subdir in dir.Folders)
                {
                    jsonWriter.WriteStartObject();
                    IndexFolder(subdir);
                    jsonWriter.WriteEndObject();
                }

                jsonWriter.WriteEndObject();
            }

            if (dir.Files.Count > 0)
            {
                jsonWriter.WritePropertyName("Files");
                jsonWriter.WriteStartArray();

                foreach (FileInfo fi in dir.Files)
                {
                    jsonWriter.WriteStartObject();

                    jsonWriter.WritePropertyName("Name");
                    jsonWriter.WriteEndObject();
                }

            }
        }
    }
}
