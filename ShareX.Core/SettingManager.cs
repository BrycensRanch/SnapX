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


using ShareX.Core.History;
using ShareX.Core.Hotkey;
using ShareX.Core.Task;
using ShareX.Core.Upload;
using ShareX.Core.Upload.Custom;
using ShareX.Core.Upload.Zip;
using ShareX.Core.Utils;

namespace ShareX.Core;

internal static class SettingManager
{
    private const string ApplicationConfigFileName = "ApplicationConfig.json";

    private static string ApplicationConfigFilePath
    {
        get
        {
            if (ShareX.Sandbox) return null;

            return Path.Combine(ShareX.PersonalFolder, ApplicationConfigFileName);
        }
    }

    private const string UploadersConfigFileName = "UploadersConfig.json";

    private static string UploadersConfigFilePath
    {
        get
        {
            if (ShareX.Sandbox) return null;

            string uploadersConfigFolder;

            if (Settings != null && !string.IsNullOrEmpty(Settings.CustomUploadersConfigPath))
            {
                uploadersConfigFolder = FileHelpers.ExpandFolderVariables(Settings.CustomUploadersConfigPath);
            }
            else
            {
                uploadersConfigFolder = ShareX.PersonalFolder;
            }

            return Path.Combine(uploadersConfigFolder, UploadersConfigFileName);
        }
    }

    private const string HotkeysConfigFileName = "HotkeysConfig.json";

    private static string HotkeysConfigFilePath
    {
        get
        {
            if (ShareX.Sandbox) return null;

            string hotkeysConfigFolder;

            if (Settings != null && !string.IsNullOrEmpty(Settings.CustomHotkeysConfigPath))
            {
                hotkeysConfigFolder = FileHelpers.ExpandFolderVariables(Settings.CustomHotkeysConfigPath);
            }
            else
            {
                hotkeysConfigFolder = ShareX.PersonalFolder;
            }

            return Path.Combine(hotkeysConfigFolder, HotkeysConfigFileName);
        }
    }

    public static string BackupFolder => Path.Combine(ShareX.PersonalFolder, "Backup");

    private static ApplicationConfig Settings { get => ShareX.Settings; set => ShareX.Settings = value; }
    private static TaskSettings DefaultTaskSettings { get => ShareX.DefaultTaskSettings; set => ShareX.DefaultTaskSettings = value; }
    private static UploadersConfig UploadersConfig { get => ShareX.UploadersConfig; set => ShareX.UploadersConfig = value; }
    private static HotkeysConfig HotkeysConfig { get => ShareX.HotkeysConfig; set => ShareX.HotkeysConfig = value; }

    private static ManualResetEvent uploadersConfigResetEvent = new ManualResetEvent(false);
    private static ManualResetEvent hotkeysConfigResetEvent = new ManualResetEvent(false);

    public static void LoadInitialSettings()
    {
        LoadApplicationConfig();

        System.Threading.Tasks.Task.Run(() =>
        {
            LoadUploadersConfig();
            uploadersConfigResetEvent.Set();

            LoadHotkeysConfig();
            hotkeysConfigResetEvent.Set();
        });
    }

    public static void WaitUploadersConfig()
    {
        if (UploadersConfig == null)
        {
            uploadersConfigResetEvent.WaitOne();
        }
    }

    public static void WaitHotkeysConfig()
    {
        if (HotkeysConfig == null)
        {
            hotkeysConfigResetEvent.WaitOne();
        }
    }

    public static void LoadApplicationConfig(bool fallbackSupport = true)
    {
        Settings = ApplicationConfig.Load(ApplicationConfigFilePath, BackupFolder, fallbackSupport);
        Settings.CreateBackup = true;
        Settings.CreateWeeklyBackup = true;
        Settings.SettingsSaveFailed += Settings_SettingsSaveFailed;
        DefaultTaskSettings = Settings.DefaultTaskSettings;
        ApplicationConfigBackwardCompatibilityTasks();
        MigrateHistoryFile();
    }

    private static void Settings_SettingsSaveFailed(Exception e)
    {
        string message;

        if (e is UnauthorizedAccessException || e is FileNotFoundException)
        {
            message = "Your antivirus software could be blocking ShareX or some other Windows feature";
        }
        else
        {
            message = e.Message;
        }
    }

    public static void LoadUploadersConfig(bool fallbackSupport = true)
    {
        UploadersConfig = UploadersConfig.Load(UploadersConfigFilePath, BackupFolder, fallbackSupport);
        UploadersConfig.CreateBackup = true;
        UploadersConfig.CreateWeeklyBackup = true;
        UploadersConfigBackwardCompatibilityTasks();
    }

    public static void LoadHotkeysConfig(bool fallbackSupport = true)
    {
        HotkeysConfig = HotkeysConfig.Load(HotkeysConfigFilePath, BackupFolder, fallbackSupport);
        HotkeysConfig.CreateBackup = true;
        HotkeysConfig.CreateWeeklyBackup = true;
        HotkeysConfigBackwardCompatibilityTasks();
    }

    public static void LoadAllSettings()
    {
        LoadApplicationConfig();
        LoadUploadersConfig();
        LoadHotkeysConfig();
    }

    private static void ApplicationConfigBackwardCompatibilityTasks()
    {
        if (Settings.IsUpgradeFrom("16.0.2"))
        {
            if (Settings.CheckPreReleaseUpdates)
            {
                Settings.UpdateChannel = UpdateChannel.PreRelease;
            }
        }
    }

    private static void MigrateHistoryFile()
    {
        if (System.IO.File.Exists(ShareX.HistoryFilePathOld))
        {
            if (!System.IO.File.Exists(ShareX.HistoryFilePath))
            {
                DebugHelper.WriteLine($"Migrating XML history file \"{ShareX.HistoryFilePathOld}\" to JSON history file \"{ShareX.HistoryFilePath}\"");

                var historyManagerXML = new HistoryManagerXML(ShareX.HistoryFilePathOld);
                var historyItems = historyManagerXML.GetHistoryItems();

                if (historyItems.Count > 0)
                {
                    var historyManagerJSON = new HistoryManagerJSON(ShareX.HistoryFilePath);
                    historyManagerJSON.AppendHistoryItems(historyItems);
                }
            }

            FileHelpers.MoveFile(ShareX.HistoryFilePathOld, BackupFolder);
        }
    }

    private static void UploadersConfigBackwardCompatibilityTasks()
    {
        if (UploadersConfig.CustomUploadersList != null)
        {
            foreach (CustomUploaderItem cui in UploadersConfig.CustomUploadersList)
            {
                cui.CheckBackwardCompatibility();
            }
        }
    }

    private static void HotkeysConfigBackwardCompatibilityTasks()
    {

        if (Settings.IsUpgradeFrom("15.0.1"))
        {
            foreach (var taskSettings in HotkeysConfig.Hotkeys.Select(x => x.TaskSettings))
            {
                if (taskSettings != null && taskSettings.CaptureSettings != null)
                {
                    // taskSettings.CaptureSettings.ScrollingCaptureOptions = new ScrollingCaptureOptions();
                    // taskSettings.CaptureSettings.FFmpegOptions.FixSources();
                }
            }
        }
    }

    public static void CleanupHotkeysConfig()
    {
        foreach (var taskSettings in HotkeysConfig.Hotkeys.Select(x => x.TaskSettings))
        {
            taskSettings.Cleanup();
        }
    }

    public static void SaveAllSettings()
    {
        if (Settings != null)
        {
            Settings.Save(ApplicationConfigFilePath);
        }

        if (UploadersConfig != null)
        {
            UploadersConfig.Save(UploadersConfigFilePath);
        }

        if (HotkeysConfig != null)
        {
            CleanupHotkeysConfig();
            HotkeysConfig.Save(HotkeysConfigFilePath);
        }
    }

    public static void SaveApplicationConfigAsync()
    {
        if (Settings != null)
        {
            Settings.SaveAsync(ApplicationConfigFilePath);
        }
    }

    public static void SaveUploadersConfigAsync()
    {
        if (UploadersConfig != null)
        {
            UploadersConfig.SaveAsync(UploadersConfigFilePath);
        }
    }

    public static void SaveHotkeysConfigAsync()
    {
        if (HotkeysConfig != null)
        {
            CleanupHotkeysConfig();
            HotkeysConfig.SaveAsync(HotkeysConfigFilePath);
        }
    }

    public static void SaveAllSettingsAsync()
    {
        SaveApplicationConfigAsync();
        SaveUploadersConfigAsync();
        SaveHotkeysConfigAsync();
    }

    public static void ResetSettings()
    {
        if (System.IO.File.Exists(ApplicationConfigFilePath)) System.IO.File.Delete(ApplicationConfigFilePath);
        LoadApplicationConfig(false);

        if (System.IO.File.Exists(UploadersConfigFilePath)) System.IO.File.Delete(UploadersConfigFilePath);
        LoadUploadersConfig(false);

        if (System.IO.File.Exists(HotkeysConfigFilePath)) System.IO.File.Delete(HotkeysConfigFilePath);
        LoadHotkeysConfig(false);
    }

    public static bool Export(string archivePath, bool settings, bool history)
    {
        MemoryStream msApplicationConfig = null, msUploadersConfig = null, msHotkeysConfig = null;

        try
        {
            var entries = new List<ZipEntryInfo>();

            if (settings)
            {
                msApplicationConfig = Settings.SaveToMemoryStream(false);
                entries.Add(new ZipEntryInfo(msApplicationConfig, ApplicationConfigFileName));

                msUploadersConfig = UploadersConfig.SaveToMemoryStream(false);
                entries.Add(new ZipEntryInfo(msUploadersConfig, UploadersConfigFileName));

                msHotkeysConfig = HotkeysConfig.SaveToMemoryStream(false);
                entries.Add(new ZipEntryInfo(msHotkeysConfig, HotkeysConfigFileName));
            }

            if (history)
            {
                entries.Add(new ZipEntryInfo(ShareX.HistoryFilePath));
            }

            ZipManager.Compress(archivePath, entries);
            return true;
        }
        catch (Exception e)
        {
            DebugHelper.WriteException(e);
        }
        finally
        {
            msApplicationConfig?.Dispose();
            msUploadersConfig?.Dispose();
            msHotkeysConfig?.Dispose();
        }

        return false;
    }

    public static bool Import(string archivePath)
    {
        try
        {
            ZipManager.Extract(archivePath, ShareX.PersonalFolder, true, entry =>
            {
                return FileHelpers.CheckExtension(entry.Name, new string[] { "json", "xml" });
            }, 1_000_000_000);

            return true;
        }
        catch (Exception e)
        {
            DebugHelper.WriteException(e);
        }

        return false;
    }
}

