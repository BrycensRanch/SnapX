
// SPDX-License-Identifier: GPL-3.0-or-later



using System.Diagnostics.CodeAnalysis;
using Esatto.Win32.Registry;
using Microsoft.Extensions.Configuration;
using SnapX.Core.History;
using SnapX.Core.Hotkey;
using SnapX.Core.Job;
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
            if (SnapX.Sandbox) return "";

            return Path.Combine(SnapX.ConfigFolder, ApplicationConfigFileName);
        }
    }

    private const string UploadersConfigFileName = "UploadersConfig.json";

    private static string UploadersConfigFilePath
    {
        get
        {
            if (SnapX.Sandbox) return "";

            string uploadersConfigFolder;

            if (!string.IsNullOrEmpty(Settings.CustomUploadersConfigPath))
            {
                uploadersConfigFolder = FileHelpers.ExpandFolderVariables(Settings.CustomUploadersConfigPath);
            }
            else
            {
                uploadersConfigFolder = SnapX.ConfigFolder;
            }

            return Path.Combine(uploadersConfigFolder, UploadersConfigFileName);
        }
    }

    private const string HotkeysConfigFileName = "HotkeysConfig.json";

    private static string HotkeysConfigFilePath
    {
        get
        {
            if (SnapX.Sandbox) return "";

            string hotkeysConfigFolder;

            if (!string.IsNullOrEmpty(Settings.CustomHotkeysConfigPath))
            {
                hotkeysConfigFolder = FileHelpers.ExpandFolderVariables(Settings.CustomHotkeysConfigPath);
            }
            else
            {
                hotkeysConfigFolder = SnapX.ConfigFolder;
            }

            return Path.Combine(hotkeysConfigFolder, HotkeysConfigFileName);
        }
    }

    public static string SnapshotFolder => Path.Combine(SnapX.PersonalFolder, "Snapshots");

    private static RootConfiguration Settings { get => SnapX.Settings; set => SnapX.Settings = value; }
    private static TaskSettings DefaultTaskSettings { get => SnapX.DefaultTaskSettings; set => SnapX.DefaultTaskSettings = value; }
    private static UploadersConfig UploadersConfig { get => SnapX.UploadersConfig; set => SnapX.UploadersConfig = value; }
    private static HotkeysConfig HotkeysConfig { get => SnapX.HotkeysConfig; set => SnapX.HotkeysConfig = value; }

    private static ManualResetEvent uploadersConfigResetEvent = new(false);
    private static ManualResetEvent hotkeysConfigResetEvent = new(false);

    public static void LoadInitialSettings()
    {
        LoadApplicationConfig();

        Task.Run(() =>
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

    [RequiresDynamicCode("Calls Microsoft.Extensions.Configuration.ConfigurationBinder.Bind(Object)")]
    [RequiresUnreferencedCode("Calls Microsoft.Extensions.Configuration.ConfigurationBinder.Bind(Object)")]
    public static void LoadApplicationConfig(bool fallbackSupport = true)
    {
        var configurationBuilder = new ConfigurationBuilder()
            // .AddInMemoryCollection()
            // Allows ALL settings to be managed via the Windows Registry.
            // This call does nothing on non-Windows Operating Systems
            .AddRegistry(@"Software\BrycensRanch\SnapX")
            .AddEnvironmentVariables(prefix: "SNAPX_")
            .AddCommandLine(Environment.GetCommandLineArgs());
        if (!SnapX.Sandbox)
        {
            configurationBuilder.AddJsonFile(ApplicationConfigFilePath, optional: true, reloadOnChange: true);
        }
        SnapX.Configuration = configurationBuilder.Build();
        var settings = new RootConfiguration();
        SnapX.Configuration.Bind(settings);
        Settings = settings;
        ApplicationConfigBackwardCompatibilityTasks();
        MigrateHistoryFile();
    }

    [RequiresDynamicCode("Calls Microsoft.Extensions.Configuration.ConfigurationBinder.Bind(Object)")]
    [RequiresUnreferencedCode("Calls Microsoft.Extensions.Configuration.ConfigurationBinder.Bind(Object)")]
    public static void LoadUploadersConfig(bool fallbackSupport = true)
    {
        var configurationBuilder = new ConfigurationBuilder()
            // .AddInMemoryCollection()
            // Allows ALL settings to be managed via the Windows Registry.
            // This call does nothing on non-Windows Operating Systems
            .AddRegistry(@"Software\BrycensRanch\SnapX")
            .AddEnvironmentVariables(prefix: "SNAPX_")
            .AddCommandLine(Environment.GetCommandLineArgs());
        if (!SnapX.Sandbox)
        {
            configurationBuilder.AddJsonFile(UploadersConfigFilePath, optional: true, reloadOnChange: true);
        }
        var BuiltConfig = configurationBuilder.Build();
        UploadersConfig = new UploadersConfig();
        BuiltConfig.Bind(UploadersConfig);
        UploadersConfigBackwardCompatibilityTasks();
    }

    [RequiresUnreferencedCode("Calls Microsoft.Extensions.Configuration.ConfigurationBinder.Bind(Object)")]
    public static void LoadHotkeysConfig(bool fallbackSupport = true)
    {
        var configurationBuilder = new ConfigurationBuilder()
            // .AddInMemoryCollection()
            // Allows ALL settings to be managed via the Windows Registry.
            // This call does nothing on non-Windows Operating Systems
            .AddRegistry(@"Software\BrycensRanch\SnapX")
            .AddEnvironmentVariables(prefix: "SNAPX_")
            .AddCommandLine(Environment.GetCommandLineArgs());
        if (!SnapX.Sandbox)
        {
            configurationBuilder.AddJsonFile(HotkeysConfigFilePath, optional: true, reloadOnChange: true);
        }
        var BuiltConfig = configurationBuilder.Build();
        HotkeysConfig = new HotkeysConfig();
        BuiltConfig.Bind(HotkeysConfig);
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
        if (File.Exists(SnapX.HistoryFilePathOld))
        {
            if (!File.Exists(SnapX.HistoryFilePath))
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
        //         if (tasktaskSettings.CaptureSettings != null)
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
        // Settings.Save(ApplicationConfigFilePath);
        // UploadersConfig.Save(UploadersConfigFilePath);
        CleanupHotkeysConfig();
        // HotkeysConfig.Save(HotkeysConfigFilePath);
    }

    public static void SaveApplicationConfigAsync()
    {
        if (Settings != null)
        {
            // Settings.SaveAsync(ApplicationConfigFilePath);
        }
    }

    public static void SaveUploadersConfigAsync()
    {
        // UploadersConfig.SaveAsync(UploadersConfigFilePath);
    }

    public static void SaveHotkeysConfigAsync()
    {
        CleanupHotkeysConfig();
        // HotkeysConfig.SaveAsync(HotkeysConfigFilePath);
    }

    public static void SaveAllSettingsAsync()
    {
        SaveApplicationConfigAsync();
        SaveUploadersConfigAsync();
        SaveHotkeysConfigAsync();
    }

    public static void ResetSettings()
    {
        if (File.Exists(ApplicationConfigFilePath)) File.Delete(ApplicationConfigFilePath);
        LoadApplicationConfig(false);

        if (File.Exists(UploadersConfigFilePath)) File.Delete(UploadersConfigFilePath);
        LoadUploadersConfig(false);

        if (File.Exists(HotkeysConfigFilePath)) File.Delete(HotkeysConfigFilePath);
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

