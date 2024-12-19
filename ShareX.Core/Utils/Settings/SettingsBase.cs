
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace ShareX.Core.Utils.Settings;

public abstract class SettingsBase<T> where T : SettingsBase<T>, new()
{
    public delegate void SettingsSavedEventHandler(T settings, string filePath, bool result);
    public event SettingsSavedEventHandler SettingsSaved;

    public delegate void SettingsSaveFailedEventHandler(Exception e);
    public event SettingsSaveFailedEventHandler SettingsSaveFailed;

    // Use JsonIgnore for properties that should not be serialized
    [JsonIgnore]
    [Browsable(false)] // Still can use Browsable if you're working with UI components
    public string FilePath { get; private set; }

    // Use JsonIgnore to prevent this property from being serialized
    [JsonIgnore]
    [Browsable(false)]
    public bool IsFirstTimeRun { get; private set; }

    // Use JsonIgnore for properties that should not be serialized
    [JsonIgnore]
    [Browsable(false)]
    public bool IsUpgrade { get; private set; }

    // This property can still be serialized
    [JsonIgnore]
    [Browsable(false)]
    public string ApplicationVersion { get; set; }

    // These properties are not ignored and will be serialized
    [JsonIgnore]
    [Browsable(false)]
    public string BackupFolder { get; set; }

    [JsonIgnore]
    [Browsable(false)]
    public bool CreateBackup { get; set; }

    [JsonIgnore]
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
        System.Threading.Tasks.Task.Run(() => Save(filePath));
    }

    public void SaveAsync()
    {
        SaveAsync(FilePath);
    }

    public MemoryStream SaveToMemoryStream(bool supportDPAPIEncryption = false)
    {
        ApplicationVersion = Helpers.GetApplicationVersion();

        var ms = new MemoryStream();
        SaveToStream(ms, true);
        return ms;
    }

    private bool SaveInternal(string filePath)
    {
        var typeName = GetType().Name;
        DebugHelper.WriteLine($"{typeName} save started: {filePath}");

        var isSuccess = false;

        try
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                lock (this)
                {
                    FileHelpers.CreateDirectoryFromFilePath(filePath);

                    var tempFilePath = filePath + ".temp";

                    using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough))
                    {
                        SaveToStream(fileStream);
                    }

                    if (!JsonHelpers.QuickVerifyJsonFile(tempFilePath))
                    {
                        throw new Exception($"{typeName} file is corrupt: {tempFilePath}");
                    }

                    if (System.IO.File.Exists(filePath))
                    {
                        string backupFilePath = null;

                        if (CreateBackup)
                        {
                            string fileName = Path.GetFileName(filePath);
                            backupFilePath = Path.Combine(BackupFolder, fileName);
                            FileHelpers.CreateDirectory(BackupFolder);
                        }

                        System.IO.File.Replace(tempFilePath, filePath, backupFilePath, true);
                    }
                    else
                    {
                        System.IO.File.Move(tempFilePath, filePath);
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
            var status = isSuccess ? "successful" : "failed";
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
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            // TypeInfoResolver = new SourceGenerationContext()
        };

        JsonSerializer.Serialize(stream, this, options);
    }

    public static T Load(string filePath, string backupFolder = null, bool fallbackSupport = true)
    {
        var fallbackFilePaths = new List<string>();

        if (fallbackSupport && !string.IsNullOrEmpty(filePath))
        {
            var tempFilePath = filePath + ".temp";
            fallbackFilePaths.Add(tempFilePath);

            if (!string.IsNullOrEmpty(backupFolder) && Directory.Exists(backupFolder))
            {
                var fileName = Path.GetFileName(filePath);
                var backupFilePath = Path.Combine(backupFolder, fileName);
                fallbackFilePaths.Add(backupFilePath);

                var fileNameNoExt = Path.GetFileNameWithoutExtension(fileName);
                var lastWeeklyBackupFilePath = Directory.GetFiles(backupFolder, fileNameNoExt + "-*").OrderBy(x => x).LastOrDefault();
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
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
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
                AllowTrailingCommas = true,
            };

            var settings = JsonSerializer.Deserialize<T>(fileStream, options);

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
