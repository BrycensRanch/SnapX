
// SPDX-License-Identifier: GPL-3.0-or-later



using Esatto.Win32.Registry;
using Microsoft.Extensions.Configuration;
using SnapX.Core.History;
using SnapX.Core.Hotkey;
using SnapX.Core.Task;
using SnapX.Core.Upload;
using SnapX.Core.Upload.Custom;
using SnapX.Core.Upload.Zip;
using SnapX.Core.Utils;

namespace SnapX.Core;

internal static class SettingManager
{
    private const string ApplicationConfigFileName = "ApplicationConfig.json";

    private static string ApplicationConfigFilePath
    {
        get
        {
            if (SnapX.Sandbox) return null;

            return Path.Combine(SnapX.ConfigFolder, ApplicationConfigFileName);
        }
    }

    private const string UploadersConfigFileName = "UploadersConfig.json";

    private static string UploadersConfigFilePath
    {
        get
        {
            if (SnapX.Sandbox) return null;

            string uploadersConfigFolder;

            // if (Settings != null && !string.IsNullOrEmpty(Settings.CustomUploadersConfigPath))
            // {
            //     uploadersConfigFolder = FileHelpers.ExpandFolderVariables(Settings.CustomUploadersConfigPath);
            // }
            // else
            // {
            uploadersConfigFolder = SnapX.ConfigFolder;
            // }

            return Path.Combine(uploadersConfigFolder, UploadersConfigFileName);
        }
    }

    private const string HotkeysConfigFileName = "HotkeysConfig.json";

    private static string HotkeysConfigFilePath
    {
        get
        {
            if (SnapX.Sandbox) return null;

            string hotkeysConfigFolder;

            // if (Settings != null && !string.IsNullOrEmpty(Settings.CustomHotkeysConfigPath))
            // {
            //     hotkeysConfigFolder = FileHelpers.ExpandFolderVariables(Settings.CustomHotkeysConfigPath);
            // }
            // else
            // {
            hotkeysConfigFolder = SnapX.ConfigFolder;
            // }

            return Path.Combine(hotkeysConfigFolder, HotkeysConfigFileName);
        }
    }

    public static string SnapshotFolder => Path.Combine(SnapX.PersonalFolder, "Snapshots");

    private static RootConfiguration Settings { get => SnapX.Settings; set => SnapX.Settings = value; }
    private static TaskSettings DefaultTaskSettings { get => SnapX.DefaultTaskSettings; set => SnapX.DefaultTaskSettings = value; }
    private static UploadersConfig UploadersConfig { get => SnapX.UploadersConfig; set => SnapX.UploadersConfig = value; }
    private static HotkeysConfig HotkeysConfig { get => SnapX.HotkeysConfig; set => SnapX.HotkeysConfig = value; }

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
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection()
            // Allows ALL settings to be managed via the Windows Registry.
            // This call does nothing on non-Windows Operating Systems
            .AddRegistry(@"Software\SnapX\SnapX")
            .AddEnvironmentVariables(prefix: "SHAREX_")
            .AddCommandLine(Environment.GetCommandLineArgs())
            .SetBasePath(SnapX.ConfigFolder);

        if (!SnapX.Sandbox)
        {
            configurationBuilder.AddJsonFile(ApplicationConfigFileName, optional: true, reloadOnChange: true);
        }
        SnapX.Configuration = configurationBuilder.Build();
        var settings = new RootConfiguration();
        // SnapX.Configuration.Bind(settings);
        SnapX.Configuration.GetSection("DefaultTaskSettings").Bind(new TaskSettings());
        SnapX.DefaultTaskSettings = SnapX.Configuration.GetSection("DefaultTaskSettings").Get<TaskSettings>();
        DebugHelper.WriteLine(DefaultTaskSettings.ToString());
        Settings = settings;
        ApplicationConfigBackwardCompatibilityTasks();
        MigrateHistoryFile();
    }

    public static void LoadUploadersConfig(bool fallbackSupport = true)
    {
        UploadersConfig = UploadersConfig.Load(UploadersConfigFilePath, SnapshotFolder, fallbackSupport);
        UploadersConfig.CreateBackup = true;
        UploadersConfig.CreateWeeklyBackup = true;
        UploadersConfigBackwardCompatibilityTasks();
    }

    public static void LoadHotkeysConfig(bool fallbackSupport = true)
    {
        HotkeysConfig = HotkeysConfig.Load(HotkeysConfigFilePath, SnapshotFolder, fallbackSupport);
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
        // if (Settings.IsUpgradeFrom("16.0.2"))
        // {
        //     if (Settings.CheckPreReleaseUpdates)
        //     {
        //         Settings.UpdateChannel = UpdateChannel.PreRelease;
        //     }
        // }
    }

    private static void MigrateHistoryFile()
    {
        if (System.IO.File.Exists(SnapX.HistoryFilePathOld))
        {
            if (!System.IO.File.Exists(SnapX.HistoryFilePath))
            {
                DebugHelper.WriteLine($"Migrating XML history file \"{SnapX.HistoryFilePathOld}\" to JSON history file \"{SnapX.HistoryFilePath}\"");

                var historyManagerXML = new HistoryManagerXML(SnapX.HistoryFilePathOld);
                var historyItems = historyManagerXML.GetHistoryItems();

                if (historyItems.Count > 0)
                {
                    var historyManagerJSON = new HistoryManagerJSON(SnapX.HistoryFilePath);
                    historyManagerJSON.AppendHistoryItems(historyItems);
                }
            }

            FileHelpers.MoveFile(SnapX.HistoryFilePathOld, SnapshotFolder);
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

        // if (Settings.IsUpgradeFrom("15.0.1"))
        // {
        //     foreach (var taskSettings in HotkeysConfig.Hotkeys.Select(x => x.TaskSettings))
        //     {
        //         if (taskSettings != null && taskSettings.CaptureSettings != null)
        //         {
        //             // taskSettings.CaptureSettings.ScrollingCaptureOptions = new ScrollingCaptureOptions();
        //             // taskSettings.CaptureSettings.FFmpegOptions.FixSources();
        //         }
        //     }
        // }
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
            // Settings.Save(ApplicationConfigFilePath);
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
        // if (Settings != null)
        // {
        //     Settings.SaveAsync(ApplicationConfigFilePath);
        // }
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
                // msApplicationConfig = Settings.SaveToMemoryStream(false);
                // entries.Add(new ZipEntryInfo(msApplicationConfig, ApplicationConfigFileName));
                //
                // msUploadersConfig = UploadersConfig.SaveToMemoryStream(false);
                // entries.Add(new ZipEntryInfo(msUploadersConfig, UploadersConfigFileName));
                //
                // msHotkeysConfig = HotkeysConfig.SaveToMemoryStream(false);
                // entries.Add(new ZipEntryInfo(msHotkeysConfig, HotkeysConfigFileName));
            }

            if (history)
            {
                entries.Add(new ZipEntryInfo(SnapX.HistoryFilePath));
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
            ZipManager.Extract(archivePath, SnapX.ConfigFolder, true, entry =>
            {
                return FileHelpers.CheckExtension(entry.Name, new[] { "json", "xml" });
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

