using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Configuration;
using SnapX.Core.CLI;
using SnapX.Core.Hotkey;
using SnapX.Core.Job;
using SnapX.Core.Upload;
using SnapX.Core.Utils;
using SnapX.Core.Utils.Extensions;
using SnapX.Core.Watch;
using Xdg.Directories;

namespace SnapX.Core;

public class SnapX
{
    public const string AppName = "SnapX";
    public static string Qualifier { get; set; }  = "";
    public const BuildType Build =

#if DEBUG
            BuildType.Debug;
#elif RPM
            BuildType.RPM;
#elif DEB
            BuildType.DEB;
#elif ARCH
            BuildType.Arch;
#elif APPIMAGE
            BuildType.AppImage;
#elif FLATPAK
            BuildType.Flatpak;
#elif SNAP
            BuildType.Snap;
#elif RUNFILE
            BuildType.Runfile;
#elif RELEASE
            BuildType.Release;
#else
            BuildType.Unknown;
#endif

    public static string VersionText
    {
        get
        {
            var version = Version.Parse(Helpers.GetApplicationVersion());
            var versionString = $"{version.Major}.{version.Minor}.{version.Revision}";
            if (version.Build > 0)
                versionString += $".{version.Build}";
            if (Settings.DevMode)
                versionString += " Dev";
            if (Environment.GetEnvironmentVariable("CONTAINER")?.ToLower() == "flatpak")
            {
                versionString += " Flatpak";
            }
            if (Environment.GetEnvironmentVariable("SNAP") != null)
            {
                versionString += " Snap";
            }
            if (Environment.GetEnvironmentVariable("APPIMAGE") != null)
            {
                versionString += " AppImage";
            }
            if (Portable)
                versionString += " Portable";

            return versionString;
        }
    }
    public void setQualifier(string qualifier) => Qualifier = qualifier;
    public static void quit()
    {
        CloseSequence();
    }
    public static string Title
    {
        get
        {
            var title = $"{AppName}{Qualifier}";

            if (Settings.DevMode)
            {
                var info = Build.ToString();

                if (IsAdmin)
                {
                    info += ", Admin";
                }

                title += $" ({info})";
            }

            return title;
        }
    }
    public static bool MultiInstance { get; private set; }
    public static bool Portable { get; private set; }
    public static bool LogToConsole { get; private set; } = true;
    public static bool SilentRun { get; private set; }
    public static bool Sandbox { get; private set; }
    public static bool IsAdmin { get; private set; }
    public static bool SteamFirstTimeConfig { get; private set; }
    public static bool IgnoreHotkeyWarning { get; private set; }
    public static bool PuushMode { get; private set; }

    internal static RootConfiguration Settings { get; set; } = new();

    internal static IConfiguration Configuration { get; set; }
    internal static TaskSettings DefaultTaskSettings { get; set; } = TaskSettings.GetDefaultTaskSettings();
    internal static UploadersConfig UploadersConfig { get; set; }
    internal static HotkeysConfig HotkeysConfig { get; set; }

    internal static Stopwatch StartTimer { get; private set; }
    internal static HotkeyManager HotkeyManager { get; set; }
    internal static WatchFolderManager WatchFolderManager { get; set; }
    public static SnapXCLIManager CLIManager { get; set; }

    #region Paths

    private const string PersonalPathConfigFileName = "PersonalPath.cfg";

    // Many Windows users consider %USERPROFILE%\Documents\SnapX the correct location,
    // and I'm not here to subvert expectations.
    public static readonly string DefaultPersonalFolder = Path.Combine(OperatingSystem.IsWindows() ? UserDirectory.DocumentsDir : BaseDirectory.DataHome, AppName);
    public static readonly string PortablePersonalFolder = FileHelpers.GetAbsolutePath(AppName);

    private static string PersonalPathConfigFilePath
    {
        get
        {
            string relativePath = FileHelpers.GetAbsolutePath(PersonalPathConfigFileName);

            if (File.Exists(relativePath))
            {
                return relativePath;
            }

            return CurrentPersonalPathConfigFilePath;
        }
    }

    private static readonly string CurrentPersonalPathConfigFilePath = Path.Combine(DefaultPersonalFolder, PersonalPathConfigFileName);

    private static readonly string PreviousPersonalPathConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppName, PersonalPathConfigFileName);

    private static readonly string PortableCheckFilePath = FileHelpers.GetAbsolutePath("Portable");
    public static EventAggregator EventAggregator { get; } = new();
    private static string CustomPersonalPath { get; set; }

    private static string CustomConfigPath { get; set; }
    public static string ShortenPath(string path) => path.Replace(Environment.GetEnvironmentVariable("HOME") ?? Environment.GetEnvironmentVariable("USERPROFILE") ?? "", "~");

    public static string PersonalFolder =>
        !string.IsNullOrEmpty(CustomPersonalPath)
            ? FileHelpers.ExpandFolderVariables(CustomPersonalPath)
            : DefaultPersonalFolder;

    public static string ConfigFolder => string.IsNullOrEmpty(CustomConfigPath)
        ? Path.Combine(
            OperatingSystem.IsWindows()
                ? UserDirectory.DocumentsDir
                : BaseDirectory.ConfigHome,
            AppName)
        : CustomConfigPath;
    public const string HistoryFileName = "History.json";

    public static string HistoryFilePath
    {
        get
        {
            if (Sandbox) return null;

            return Path.Combine(PersonalFolder, HistoryFileName);
        }
    }

    public const string HistoryFileNameOld = "History.xml";

    public static string HistoryFilePathOld
    {
        get
        {
            if (Sandbox) return null;

            return Path.Combine(PersonalFolder, HistoryFileNameOld);
        }
    }

    public const string LogsFolderName = "Logs";
    // On Linux, strictly adhere to XDG BaseDirectory spec.
    // On macOS, most of these XDG directories resolve to $HOME/Library/Application Support	anyways so it doesn't really matter.
    public static string LogsFolder => OperatingSystem.IsLinux() ? Path.Combine(BaseDirectory.StateHome, AppName, LogsFolderName) : Path.Combine(PersonalFolder, LogsFolderName);

    public static string LogsFilePath
    {
        get
        {
            if (Settings.DisableLogging)
            {
                return string.Empty;
            }
            var date = DateTime.Now;
            return Path.Combine(LogsFolder, date.Year.ToString(), date.Month.ToString("D2"), $"SnapX-{date.Day}.log");
        }
    }


    public static string ScreenshotsParentFolder
    {
        get
        {
            if (Settings != null && Settings.UseCustomScreenshotsPath)
            {
                string path = Settings.CustomScreenshotsPath;
                string path2 = Settings.CustomScreenshotsPath2;
                if (!string.IsNullOrEmpty(path))
                {
                    path = FileHelpers.ExpandFolderVariables(path);

                    if (string.IsNullOrEmpty(path2) || Directory.Exists(path))
                    {
                        return path;
                    }
                }

                if (!string.IsNullOrEmpty(path2))
                {
                    path2 = FileHelpers.ExpandFolderVariables(path2);

                    if (Directory.Exists(path2))
                    {
                        return path2;
                    }
                }
            }

            return Path.Combine(PersonalFolder, "Screenshots");
        }
    }

    public static string ImageEffectsFolder => Path.Combine(PersonalFolder, "ImageEffects");

    private static string PersonalPathDetectionMethod;

    #endregion Paths

    private static bool closeSequenceStarted, restartRequested, restartAsAdmin;

    public void start()
    {
        start(Array.Empty<string>());
    }

    public void silenceLogging()
    {
        LogToConsole = false;
    }

    public void shutdown()
    {
        CloseSequence();
    }
    public void start(string[] args)
    {
        HandleExceptions();

        StartTimer = Stopwatch.StartNew();
        // TODO: Implement CLI in a better way than what it is now.
        CLIManager = new SnapXCLIManager(args);
        CLIManager.ParseCommands();
        CLIManager.UseCommandLineArgs(CLIManager.Commands).GetAwaiter().GetResult();

        if (CheckAdminTasks()) return; // If SnapX opened just for be able to execute a task as Admin

        UpdatePersonalPath();
        if (CLIManager.IsCommandExist("noconsole")) LogToConsole = false;

        DebugHelper.Init(LogsFilePath);

        MultiInstance = CLIManager.IsCommandExist("multi", "m");
        if (CLIManager.IsCommandExist("sound", "s"))
        {
            DebugHelper.WriteLine("Running Sound Command");
            PlayNotificationSoundAsync(NotificationSound.ActionCompleted);
        }
        Run();
    }

    public long getStartupTime() => StartTimer.ElapsedMilliseconds;
    public EventAggregator getEventAggregator() => EventAggregator;
    public bool isSilent() => SilentRun;

    // Supports the failed standard https://consoledonottrack.com/
    public static bool TelemetryEnabled() => !FeatureFlags.DisableTelemetry && !Settings.DisableTelemetry &&
                                    Environment.GetEnvironmentVariable("DO_NOT_TRACK") == null;

    // Coding nerds, please, forgive me for this mortal sin.
    // The code here is instance dependent thus cannot be called from static stuff yada yada yada.
    public void PlayNotificationSoundAsync(NotificationSound notificationSound, TaskSettings? taskSettings = null)
    {
        if (taskSettings == null) taskSettings = TaskSettings.GetDefaultTaskSettings();
        switch (notificationSound)
        {
            case NotificationSound.Capture:
                if (taskSettings.GeneralSettings.PlaySoundAfterCapture)
                {
                    if (taskSettings.GeneralSettings.UseCustomCaptureSound && !string.IsNullOrEmpty(taskSettings.GeneralSettings.CustomCaptureSoundPath))
                    {
                        PlaySound(taskSettings.GeneralSettings.CustomCaptureSoundPath);
                    }
                    else
                    {
                        PlaySound(Resources.Resources.CaptureSound);
                    }
                }
                break;
            case NotificationSound.TaskCompleted:
                if (taskSettings.GeneralSettings.PlaySoundAfterUpload)
                {
                    if (taskSettings.GeneralSettings.UseCustomTaskCompletedSound && !string.IsNullOrEmpty(taskSettings.GeneralSettings.CustomTaskCompletedSoundPath))
                    {
                        PlaySound(taskSettings.GeneralSettings.CustomTaskCompletedSoundPath);
                    }
                    else
                    {
                        PlaySound(Resources.Resources.TaskCompletedSound);
                    }
                }
                break;
            case NotificationSound.ActionCompleted:
                if (taskSettings.GeneralSettings.PlaySoundAfterAction)
                {
                    if (taskSettings.GeneralSettings.UseCustomActionCompletedSound && !string.IsNullOrEmpty(taskSettings.GeneralSettings.CustomActionCompletedSoundPath))
                    {
                        PlaySound(taskSettings.GeneralSettings.CustomActionCompletedSoundPath);
                    }
                    else
                    {
                        PlaySound(Resources.Resources.ActionCompletedSound);
                    }
                }
                break;
            case NotificationSound.Error:
                if (taskSettings.GeneralSettings.PlaySoundAfterUpload)
                {
                    if (taskSettings.GeneralSettings.UseCustomErrorSound && !string.IsNullOrEmpty(taskSettings.GeneralSettings.CustomErrorSoundPath))
                    {
                        PlaySound(taskSettings.GeneralSettings.CustomErrorSoundPath);
                    }
                    else
                    {
                        PlaySound(Resources.Resources.ErrorSound);
                    }
                }
                break;
        }
    }
    private static void Run()
    {
        DebugHelper.WriteLine("SnapX starting.");
        DebugHelper.WriteLine("Version: " + VersionText);
        DebugHelper.WriteLine("Build: " + Build);
        DebugHelper.WriteLine("Data folder: " + ShortenPath(PersonalFolder));
        DebugHelper.WriteLine("Config folder: " + ShortenPath(ConfigFolder));

        if (!string.IsNullOrWhiteSpace(PersonalPathDetectionMethod))
        {
            DebugHelper.WriteLine("Personal path detection method: " + PersonalPathDetectionMethod);
        }
        DebugHelper.WriteLine("Operating system: " + Helpers.GetOperatingSystemProductName(true));
        if (OperatingSystem.IsLinux())
        {
            var sessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE") ?? "Unknown";
            var desktopEnvironment = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") ?? "Unknown";
            var kdePlasmaMajorVersion = Environment.GetEnvironmentVariable("KDE_SESSION_VERSION");
            DebugHelper.WriteLine($"Session Type: {sessionType}");
            DebugHelper.WriteLine($"Desktop Environment: {desktopEnvironment}{(desktopEnvironment == "KDE" ? $" {kdePlasmaMajorVersion}" : "")}");
        }
        DebugHelper.WriteLine($"Platform: {Environment.OSVersion.Platform} {Environment.OSVersion.Version}");
        if (OperatingSystem.IsLinux() && OsInfo.IsWSL()) DebugHelper.WriteLine("Running under WSL. Please keep in mind that SnapX defaults to escaping WSL. You can turn this off in settings.");
        DebugHelper.WriteLine(".NET: " + RuntimeInformation.FrameworkDescription);

        _ = Task.Run(() =>
        {
            DebugHelper.WriteLine($"CPU: {OsInfo.GetProcessorName()} ({Environment.ProcessorCount})");
            var (totalMemory, usedMemory) = OsInfo.GetMemoryInfo();
            DebugHelper.WriteLine($"Total Memory: {totalMemory} MiB");
            DebugHelper.WriteLine($"Used Memory: {usedMemory} MiB");
            OsInfo.PrintGraphicsInfo();
            // Linux is not supported for HDR detection.
            if (!OperatingSystem.IsLinux()) DebugHelper.WriteLine($"HDR Capable: {OsInfo.IsHdrSupported()}");
        });
        IsAdmin = Helpers.IsAdministrator();
        DebugHelper.WriteLine("Running as elevated process: " + IsAdmin);

        SilentRun = CLIManager.IsCommandExist("silent", "s");

        IgnoreHotkeyWarning = CLIManager.IsCommandExist("NoHotkeys");

        CreateParentFolders();
        RegisterIntegrations();
        CheckPuushMode();
        DebugWriteFlags();
        // SettingManager.LoadInitialSettings();
        SettingManager.LoadAllSettings();
        if (TelemetryEnabled())
            SentrySdk.Init(options =>
            {
                // This allows end users to test themselves what data is sent to Sentry
                var sentryDsnEnv = Environment.GetEnvironmentVariable("SENTRY_DSN");
                options.Dsn = !string.IsNullOrWhiteSpace(sentryDsnEnv) ? sentryDsnEnv : "https://e0a07df30c8b96560f93b10cf4338eba@o4504136997928960.ingest.us.sentry.io/4508785180737536";

                // When debug is enabled, the Sentry client will emit detailed debugging information to the console.
                options.Debug = Environment.GetEnvironmentVariable("SENTRY_DEBUG") == "1";
                // VLCException includes multiple paths with username
                // For full transparency, I discovered this issue on my computer.
                // No other users are effected to my knowledge.
                options.SetBeforeSend((sentryEvent, hint) =>
                {
                    if (sentryEvent.Exception != null
                        && !string.IsNullOrEmpty(sentryEvent.Exception.Message))
                    {
                        if (sentryEvent.Exception.Message.Contains(Environment.UserName)) return null;
                    }

                    return sentryEvent;
                });

                // Enabling this option is recommended for client applications only. It ensures all threads use the same global scope.
                options.IsGlobalModeEnabled = true;

                // This option is recommended. It enables Sentry's "Release Health" feature.
                options.AutoSessionTracking = true;

                // Set TracesSampleRate to 1.0 to capture 100%
                // of transactions for tracing.
                options.TracesSampleRate = 0.2;

                // Sample rate for profiling, applied on top of the TracesSampleRate,
                // e.g. 0.2 means we want to profile 20 % of the captured transactions.
                // We recommend adjusting this value in production.
                options.ProfilesSampleRate = 0.2;
                options.AddIntegration(new ProfilingIntegration());

                // This saves events for later when internet connectivity is poor/not working.
                options.CacheDirectoryPath = Path.Combine(BaseDirectory.CacheHome, AppName);
            });
        if (CLIManager.IsCommandExist("noconsole")) LogToConsole = false;
        // CleanupManager.CleanupAsync();

    }

    public CLIManager GetCLIManager() => CLIManager;
    // TODO: Implement Dependency Injection to pass around instance of SnapX to classes
    // TODO: Add back all notification sounds calls
    public async virtual Task PlaySound(Stream stream) => throw new NotImplementedException("PlaySound is not implemented.");
    private async Task PlaySound(string filePath) => await PlaySound(File.OpenRead(filePath));
    public static void CloseSequence()
    {
        if (closeSequenceStarted) return;
        closeSequenceStarted = true;

        DebugHelper.WriteLine("SnapX closing.");

        WatchFolderManager?.Dispose();
        SettingManager.SaveAllSettings();
        if (TelemetryEnabled()) SentrySdk.Close();

        DebugHelper.WriteLine("SnapX closed.");
        DebugHelper.FlushBufferedMessages();
        Environment.Exit(0);
    }

    private static void UpdatePersonalPath()
    {
        if (Sandbox) return;
        Sandbox = CLIManager.IsCommandExist("sandbox");

        if (CLIManager.IsCommandExist("portable", "p"))
        {
            Portable = true;
            CustomPersonalPath = PortablePersonalFolder;
            PersonalPathDetectionMethod = "Portable CLI flag";
        }
        if (File.Exists(PortableCheckFilePath))
        {
            Portable = true;
            CustomPersonalPath = PortablePersonalFolder;
            PersonalPathDetectionMethod = $"Portable file ({PortableCheckFilePath})";
        }
        else
        {
            MigratePersonalPathConfig();

            string customPersonalPath = ReadPersonalPathConfig();

            if (!string.IsNullOrEmpty(customPersonalPath))
            {
                CustomPersonalPath = FileHelpers.GetAbsolutePath(customPersonalPath);
                PersonalPathDetectionMethod = $"PersonalPath.cfg file ({PersonalPathConfigFilePath})";
            }
        }

        if (!Directory.Exists(PersonalFolder))
        {
            try
            {
                Directory.CreateDirectory(PersonalFolder);
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();

                sb.AppendFormat("{0} \"{1}\"", "Unable to create personal folder!", PersonalFolder);
                sb.AppendLine();

                if (!string.IsNullOrEmpty(PersonalPathDetectionMethod))
                {
                    sb.AppendLine("Personal path detection method: " + PersonalPathDetectionMethod);
                }

                sb.AppendLine();
                sb.Append(e);

                CustomPersonalPath = "";
            }
        }
        if (!Directory.Exists(ConfigFolder)) FileHelpers.CreateDirectory(ConfigFolder);
    }

    private static void CreateParentFolders()
    {
        if (!Sandbox && Directory.Exists(PersonalFolder))
        {
            FileHelpers.CreateDirectory(SettingManager.SnapshotFolder);
            FileHelpers.CreateDirectory(ImageEffectsFolder);
            FileHelpers.CreateDirectory(ScreenshotsParentFolder);
        }
    }

    private static void RegisterIntegrations()
    {
        if (Portable || Sandbox) return;
#if WINDOWS
        // TODO: Reimplement FirstTimeForm to give users chance to consent
        if (!WindowsAPI.CheckCustomUploaderExtension()) WindowsAPI.CreateCustomUploaderExtension(true);
        if (!WindowsAPI.CheckImageEffectExtension()) WindowsAPI.CreateImageEffectExtension(true);
        if (!WindowsAPI.CheckShellContextMenuButton()) WindowsAPI.CreateShellContextMenuButton(true);
        if (!WindowsAPI.CheckSendToMenuButton()) WindowsAPI.CreateSendToMenuButton(true);

        if (!WindowsAPI.CheckChromeExtensionSupport()) WindowsAPI.CreateChromeExtensionSupport(true);
        if (!WindowsAPI.CheckFirefoxAddonSupport())
            WindowsAPI.CreateFirefoxAddonSupport(true);
#endif
    }

    private static void MigratePersonalPathConfig()
    {
        if (!OperatingSystem.IsWindows())
        {
            try
            {
                // @see https://github.com/BrycensRanch/SnapX-Linux-Port/blob/c650e315ab51e9100e4c63d61e5915fcf530d96c/Progress.md
                var InformalPath = Path.Join(UserDirectory.DocumentsDir, AppName);
                if (Directory.Exists(UserDirectory.DocumentsDir) && !File.Exists(InformalPath)) Directory.CreateSymbolicLink(InformalPath, PersonalFolder);
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException)
                {
                    return;
                }
                if (e.HResult == -2147024816 ||
                    e.HResult == -2147024713) {
                    // The file already exists, ignore.
                   return;
                }
                DebugHelper.WriteLine("Failed to symbolic link typical SnapX path. You can safely ignore this.");
                DebugHelper.WriteException(e);
            }
        }
        if (File.Exists(PreviousPersonalPathConfigFilePath))
        {
            try
            {
                if (!File.Exists(CurrentPersonalPathConfigFilePath))
                {
                    FileHelpers.CreateDirectoryFromFilePath(CurrentPersonalPathConfigFilePath);
                    FileHelpers.CreateDirectoryFromFilePath(ConfigFolder);
                    File.Move(PreviousPersonalPathConfigFilePath, CurrentPersonalPathConfigFilePath);
                }
                File.Delete(PreviousPersonalPathConfigFilePath);
                Directory.Delete(Path.GetDirectoryName(PreviousPersonalPathConfigFilePath));
            }
            catch (Exception e)
            {
                e.ShowError();
            }
        }
    }

    public static string ReadPersonalPathConfig()
    {
        return File.Exists(PersonalPathConfigFilePath)
            ? File.ReadAllText(PersonalPathConfigFilePath, Encoding.UTF8).Trim()
            : string.Empty;
    }

    public static bool WritePersonalPathConfig(string path)
    {
        path = path?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(path) && !File.Exists(PersonalPathConfigFilePath))
            return false;

        var currentPath = ReadPersonalPathConfig();

        if (path.Equals(currentPath, StringComparison.OrdinalIgnoreCase))
            return false;

        try
        {
            FileHelpers.CreateDirectoryFromFilePath(PersonalPathConfigFilePath);
            File.WriteAllText(PersonalPathConfigFilePath, path, Encoding.UTF8);
            return true;
        }
        catch (UnauthorizedAccessException e)
        {
            DebugHelper.WriteException(e);
        }
        catch (Exception e)
        {
            DebugHelper.WriteException(e);
            e.ShowError();
        }

        return false;
    }

    private static void HandleExceptions()
    {
#if DEBUG
        if (Debugger.IsAttached)
        {
            return;
        }
#endif

        // Add the event handler for handling non-UI thread exceptions to the event
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e) => OnError(e.Exception);
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) => OnError((Exception)e.ExceptionObject);
    private static void OnError(Exception e) => DebugHelper.WriteException(e);

    private static bool CheckAdminTasks()
    {
        if (CLIManager.IsCommandExist("dnschanger"))
        {
            return true;
        }

        return false;
    }

    private static bool CheckPuushMode()
    {
        var puushPath = FileHelpers.GetAbsolutePath("puush");
        PuushMode = File.Exists(puushPath);
        return PuushMode;
    }

    private static void DebugWriteFlags()
    {
        var flags = new List<string>();

        if (Settings.DevMode) flags.Add(nameof(Settings.DevMode));
        if (MultiInstance) flags.Add(nameof(MultiInstance));
        if (Portable) flags.Add(nameof(Portable));
        if (SilentRun) flags.Add(nameof(SilentRun));
        if (Sandbox) flags.Add(nameof(Sandbox));
        if (IgnoreHotkeyWarning) flags.Add(nameof(IgnoreHotkeyWarning));
        if (FeatureFlags.DisableTelemetry) flags.Add(nameof(FeatureFlags.DisableTelemetry));
        if (FeatureFlags.DisableAutoUpdates) flags.Add(nameof(FeatureFlags.DisableAutoUpdates));
        if (FeatureFlags.DisableUploads) flags.Add(nameof(FeatureFlags.DisableUploads));
        if (PuushMode) flags.Add(nameof(PuushMode));

        var output = string.Join(", ", flags);
        DebugHelper.WriteLine("Flags: " + output);
    }
}

