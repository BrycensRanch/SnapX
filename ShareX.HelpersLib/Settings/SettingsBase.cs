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

using System.ComponentModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace ShareX.HelpersLib
{
    public abstract class SettingsBase<T> where T : SettingsBase<T>, new()
    {
        public delegate void SettingsSavedEventHandler(T settings, string filePath, bool result);
        public event SettingsSavedEventHandler SettingsSaved;

        public delegate void SettingsSaveFailedEventHandler(Exception e);
        public event SettingsSaveFailedEventHandler SettingsSaveFailed;

        // Use JsonIgnore for properties that should not be serialized
        [System.Text.Json.Serialization.JsonIgnore]
        [Browsable(false)] // Still can use Browsable if you're working with UI components
        public string FilePath { get; private set; }

        // Use JsonIgnore to prevent this property from being serialized
        [System.Text.Json.Serialization.JsonIgnore]
        [Browsable(false)]
        public bool IsFirstTimeRun { get; private set; }

        // Use JsonIgnore for properties that should not be serialized
        [System.Text.Json.Serialization.JsonIgnore]
        [Browsable(false)]
        public bool IsUpgrade { get; private set; }

        // This property can still be serialized
        [System.Text.Json.Serialization.JsonIgnore]
        [Browsable(false)]
        public string ApplicationVersion { get; set; }

        // These properties are not ignored and will be serialized
        [System.Text.Json.Serialization.JsonIgnore]
        [Browsable(false)]
        public string BackupFolder { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Browsable(false)]
        public bool CreateBackup { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Browsable(false)]
        public bool CreateWeeklyBackup { get; set; }


        public bool IsUpgradeFrom(string version)
        {
            return IsUpgrade && Helpers.CompareVersion(ApplicationVersion, version) <= 0;
        }

        protected virtual void OnSettingsSaved(string filePath, bool result)
        {
            SettingsSaved?.Invoke((T)this, filePath, result);
        }

        protected virtual void OnSettingsSaveFailed(Exception e)
        {
            SettingsSaveFailed?.Invoke(e);
        }

        public bool Save(string filePath)
        {
            FilePath = filePath;
            ApplicationVersion = Helpers.GetApplicationVersion();

            bool result = SaveInternal(FilePath);

            OnSettingsSaved(FilePath, result);

            return result;
        }

        public bool Save()
        {
            return Save(FilePath);
        }

        public void SaveAsync(string filePath)
        {
            Task.Run(() => Save(filePath));
        }

        public void SaveAsync()
        {
            SaveAsync(FilePath);
        }

        public MemoryStream SaveToMemoryStream(bool supportDPAPIEncryption = false)
        {
            ApplicationVersion = Helpers.GetApplicationVersion();

            MemoryStream ms = new MemoryStream();
            SaveToStream(ms, true);
            return ms;
        }

        private bool SaveInternal(string filePath)
        {
            string typeName = GetType().Name;
            DebugHelper.WriteLine($"{typeName} save started: {filePath}");

            bool isSuccess = false;

            try
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    lock (this)
                    {
                        FileHelpers.CreateDirectoryFromFilePath(filePath);

                        string tempFilePath = filePath + ".temp";

                        using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough))
                        {
                            SaveToStream(fileStream);
                        }

                        if (!JsonHelpers.QuickVerifyJsonFile(tempFilePath))
                        {
                            throw new Exception($"{typeName} file is corrupt: {tempFilePath}");
                        }

                        if (File.Exists(filePath))
                        {
                            string backupFilePath = null;

                            if (CreateBackup)
                            {
                                string fileName = Path.GetFileName(filePath);
                                backupFilePath = Path.Combine(BackupFolder, fileName);
                                FileHelpers.CreateDirectory(BackupFolder);
                            }

                            File.Replace(tempFilePath, filePath, backupFilePath, true);
                        }
                        else
                        {
                            File.Move(tempFilePath, filePath);
                        }

                        if (CreateWeeklyBackup && !string.IsNullOrEmpty(BackupFolder))
                        {
                            FileHelpers.BackupFileWeekly(filePath, BackupFolder);
                        }

                        isSuccess = true;
                    }
                }
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);

                OnSettingsSaveFailed(e);
            }
            finally
            {
                string status = isSuccess ? "successful" : "failed";
                DebugHelper.WriteLine($"{typeName} save {status}: {filePath}");
            }

            return isSuccess;
        }

        private void SaveToStream(Stream stream, bool leaveOpen = false)
        {
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() },
                DefaultIgnoreCondition = JsonIgnoreCondition.Never

            };

            using var jsonWriter = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Indented = true
            });

            JsonSerializer.Serialize(jsonWriter, this, options);
        }

        public static T Load(string filePath, string backupFolder = null, bool fallbackSupport = true)
        {
            List<string> fallbackFilePaths = new List<string>();

            if (fallbackSupport && !string.IsNullOrEmpty(filePath))
            {
                string tempFilePath = filePath + ".temp";
                fallbackFilePaths.Add(tempFilePath);

                if (!string.IsNullOrEmpty(backupFolder) && Directory.Exists(backupFolder))
                {
                    string fileName = Path.GetFileName(filePath);
                    string backupFilePath = Path.Combine(backupFolder, fileName);
                    fallbackFilePaths.Add(backupFilePath);

                    string fileNameNoExt = Path.GetFileNameWithoutExtension(fileName);
                    string lastWeeklyBackupFilePath = Directory.GetFiles(backupFolder, fileNameNoExt + "-*").OrderBy(x => x).LastOrDefault();
                    if (!string.IsNullOrEmpty(lastWeeklyBackupFilePath))
                    {
                        fallbackFilePaths.Add(lastWeeklyBackupFilePath);
                    }
                }
            }

            var setting = LoadInternal<T>(filePath, fallbackFilePaths);

            if (setting != null)
            {
                setting.FilePath = filePath;
                setting.IsFirstTimeRun = string.IsNullOrEmpty(setting.ApplicationVersion);
                setting.IsUpgrade = !setting.IsFirstTimeRun && Helpers.CompareApplicationVersion(setting.ApplicationVersion) < 0;
                setting.BackupFolder = backupFolder;
            }

            return setting;
        }
        private static T LoadInternal<T>(string filePath, List<string> fallbackFilePaths = null)
        {
            string typeName = typeof(T).Name;

            // Guard clause for invalid file path
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                DebugHelper.WriteLine($"{typeName} file does not exist: {filePath}");
                return TryLoadFromFallback<T>(fallbackFilePaths, typeName);
            }

            DebugHelper.WriteLine($"{typeName} load started: {filePath}");

            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                // Guard clause for empty file stream
                if (fileStream.Length == 0)
                {
                    throw new Exception($"{typeName} file stream length is 0.");
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                T settings = JsonSerializer.Deserialize<T>(fileStream, options);

                // Guard clause for null settings
                if (settings is null)
                {
                    throw new Exception($"{typeName} object is null.");
                }

                DebugHelper.WriteLine($"{typeName} load finished: {filePath}");
                return settings;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteException(ex, $"{typeName} load failed: {filePath}");
                return TryLoadFromFallback<T>(fallbackFilePaths, typeName);
            }
        }
        private static T TryLoadFromFallback<T>(List<string> fallbackFilePaths, string typeName)
        {
            if (fallbackFilePaths?.Count > 0)
            {
                var fallbackFilePath = fallbackFilePaths[0];
                fallbackFilePaths.RemoveAt(0);
                return LoadInternal<T>(fallbackFilePath, fallbackFilePaths);
            }

            DebugHelper.WriteLine($"Loading new {typeName} instance.");
            return Activator.CreateInstance<T>();
        }
    }
}
