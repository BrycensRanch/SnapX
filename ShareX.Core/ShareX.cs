using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Configuration;
using ScreenCapture.NET;
using ShareX.Core.CLI;
using ShareX.Core.Hotkey;
using ShareX.Core.Task;
using ShareX.Core.Upload;
using ShareX.Core.Utils;
using ShareX.Core.Utils.Extensions;
using ShareX.Core.Watch;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xdg.Directories;

namespace ShareX.Core;

public class ShareX
{
    public const string AppName = "ShareX";
    public static string Qualifier = "";
    public const ShareXBuild Build =
#if RELEASE
            ShareXBuild.Release;
#elif DEBUG
            ShareXBuild.Debug;
#else
            ShareXBuild.Unknown;
#endif

    public static string VersionText
    {
        get
        {
            var version = Version.Parse(Helpers.GetApplicationVersion());
            var versionString = $"{version.Major}.{version.Minor}.{version.Revision}";
            if (version.Build > 0)
                versionString += $".{version.Build}";
            // if (Settings.DevMode)
            //     versionString += " Dev";
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

            // if (Settings.DevMode)
            // {
            //     var info = Build.ToString();
            //
            //     if (IsAdmin)
            //     {
            //         info += ", Admin";
            //     }
            //
            //     title += $" ({info})";
            // }

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

    internal static RootConfiguration Settings { get; set; }

    internal static IConfiguration Configuration { get; set; }
    internal static TaskSettings DefaultTaskSettings { get; set; }
    internal static UploadersConfig UploadersConfig { get; set; }
    internal static HotkeysConfig HotkeysConfig { get; set; }

    internal static Stopwatch StartTimer { get; private set; }
    internal static HotkeyManager HotkeyManager { get; set; }
    internal static WatchFolderManager WatchFolderManager { get; set; }
    public static CLIManager CLIManager { get; set; }

    #region Paths

    private const string PersonalPathConfigFileName = "PersonalPath.cfg";

    // Many Windows users consider %USERPROFILE%\Documents\ShareX the correct location,
    // and I'm not here to subvert expectations.
    public static readonly string DefaultPersonalFolder = Path.Combine(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? UserDirectory.DocumentsDir : BaseDirectory.DataHome, AppName);
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

    private static string CustomPersonalPath { get; set; }

    public static string PersonalFolder
    {
        get
        {
            if (!string.IsNullOrEmpty(CustomPersonalPath))
            {
                return FileHelpers.ExpandFolderVariables(CustomPersonalPath);
            }

            return DefaultPersonalFolder;
        }
    }

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

    public static string LogsFolder => Path.Combine(PersonalFolder, LogsFolderName);

    public static string LogsFilePath
    {
        get
        {
            // if (SystemOptions.DisableLogging)
            // {
            //     return null;
            // }
            var date = DateTime.Now;
            return Path.Combine(LogsFolder, date.Year.ToString(), date.Month.ToString("D2"), $"ShareX-{date.Day}.log");
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
        CLIManager = new CLIManager(args);
        CLIManager.ParseCommands();

        if (CheckAdminTasks()) return; // If ShareX opened just for be able to execute task as Admin

        UpdatePersonalPath();
        if (CLIManager.IsCommandExist("noconsole")) LogToConsole = false;

        DebugHelper.Init(LogsFilePath);

        MultiInstance = CLIManager.IsCommandExist("multi", "m");
        Run();
    }

    public long getStartupTime() => StartTimer.ElapsedMilliseconds;

    public bool isSilent() => SilentRun;

    private static void Run()
    {
        DebugHelper.WriteLine("ShareX (Linux) starting.");
        DebugHelper.WriteLine("Version: " + VersionText);
        DebugHelper.WriteLine("Build: " + Build);
        DebugHelper.WriteLine("Command line: " + Environment.CommandLine);
        DebugHelper.WriteLine("Personal path: " + PersonalFolder);
        if (!string.IsNullOrWhiteSpace(PersonalPathDetectionMethod))
        {
            DebugHelper.WriteLine("Personal path detection method: " + PersonalPathDetectionMethod);
        }
        DebugHelper.WriteLine("Operating system: " + Helpers.GetOperatingSystemProductName(true));
        IsAdmin = Helpers.IsAdministrator();
        DebugHelper.WriteLine("Running as elevated process: " + IsAdmin);

        SilentRun = CLIManager.IsCommandExist("silent", "s");

        IgnoreHotkeyWarning = CLIManager.IsCommandExist("NoHotkeys");

        CreateParentFolders();
        RegisterExtensions();
        CheckPuushMode();
        DebugWriteFlags();
        // SettingManager.LoadInitialSettings();
        SettingManager.LoadAllSettings();
        if (CLIManager.IsCommandExist("screenshot", "ss"))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var screenCaptureService = new DX11ScreenCaptureService();
                var graphicsCards = screenCaptureService.GetGraphicsCards();
                var firstGraphicsCard = graphicsCards.First<GraphicsCard>();
                var displays = screenCaptureService.GetDisplays(firstGraphicsCard);
                Console.WriteLine($"First graphics card: {firstGraphicsCard.Name} ({firstGraphicsCard.VendorId})");
                var screenCapture = screenCaptureService.GetScreenCapture(displays.First());
                var fullscreen = screenCapture.RegisterCaptureZone(0, 0, screenCapture.Display.Width, screenCapture.Display.Height);
                screenCapture.CaptureScreen();

                using (fullscreen.Lock())
                {
                    var rawData = fullscreen.RawBuffer;

                    using var theImage = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(rawData, fullscreen.Width, fullscreen.Height);

                    theImage.SaveAsPng(Path.Combine(ScreenshotsParentFolder, "demo.png"));
                    var imageWithRightTypes = theImage.CloneAs<Rgba64>();
                    UploadManager.UploadImage(imageWithRightTypes);


                }

            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") != null) Console.Error.WriteLine("Wayland support has not been added yet. This will likely fail with a mysterious error.");
                var screenCaptureService = new X11ScreenCaptureService();
                var graphicsCards = screenCaptureService.GetGraphicsCards();
                foreach (var card in graphicsCards)
                {
                    Console.WriteLine(card.Index);
                    Console.WriteLine(card.Name);
                    Console.WriteLine(card.VendorId);
                    Console.WriteLine(card.DeviceId);
                }
                var firstGraphicsCard = graphicsCards.First<GraphicsCard>();
                Console.WriteLine($"First graphics card: {firstGraphicsCard.Name} ({firstGraphicsCard.VendorId})");
                var displays = screenCaptureService.GetDisplays(firstGraphicsCard);
                foreach (var display in displays)
                {
                    Console.WriteLine(display.DeviceName);
                    Console.WriteLine(display.Width + " x " + display.Height);
                }
                var screenCapture = screenCaptureService.GetScreenCapture(displays.First());
                var fullscreen = screenCapture.RegisterCaptureZone(0, 0, screenCapture.Display.Width, screenCapture.Display.Height);
                screenCapture.CaptureScreen();

                //Lock the zone to access the data. Remember to dispose the returned disposable to unlock again.
                lock (fullscreen)
                {
                    var rawData = fullscreen.RawBuffer;

                    Console.WriteLine(fullscreen.Width + " x " + fullscreen.Height);
                    using var theImage = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(rawData, fullscreen.Width, fullscreen.Height);
                    var imageWithRightTypes = theImage.CloneAs<Rgba64>();

                    theImage.SaveAsPng(Path.Combine(ScreenshotsParentFolder, "demo.png"));
                    UploadManager.UploadImage(imageWithRightTypes);
                }
            }
            else
            {
                throw new UnauthorizedAccessException("Only the good Operating Systems can take screenshots");
            }
        }
        // CleanupManager.CleanupAsync();

    }

    public static void CloseSequence()
    {
        if (closeSequenceStarted) return;
        closeSequenceStarted = true;

        DebugHelper.WriteLine("ShareX closing.");

        WatchFolderManager?.Dispose();
        SettingManager.SaveAllSettings();

        DebugHelper.WriteLine("ShareX closed.");
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
        // else if (!string.IsNullOrEmpty(SystemOptions.PersonalPath))
        // {
        //     CustomPersonalPath = SystemOptions.PersonalPath;
        //     PersonalPathDetectionMethod = "Registry";
        // }
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
                StringBuilder sb = new StringBuilder();

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
    }

    private static void CreateParentFolders()
    {
        if (!Sandbox && Directory.Exists(PersonalFolder))
        {
            FileHelpers.CreateDirectory(SettingManager.BackupFolder);
            FileHelpers.CreateDirectory(ImageEffectsFolder);
            FileHelpers.CreateDirectory(ScreenshotsParentFolder);
        }
    }

    private static void RegisterExtensions()
    {
    }

    private static void MigratePersonalPathConfig()
    {
        DebugHelper.WriteLine("MigratePersonalPathConfig called");
        if (File.Exists(PreviousPersonalPathConfigFilePath))
        {
            try
            {
                if (!File.Exists(CurrentPersonalPathConfigFilePath))
                {
                    FileHelpers.CreateDirectoryFromFilePath(CurrentPersonalPathConfigFilePath);
                    File.Move(PreviousPersonalPathConfigFilePath, CurrentPersonalPathConfigFilePath);
                }
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    try
                    {
                        // @see https://github.com/BrycensRanch/ShareX-Linux-Port/blob/c650e315ab51e9100e4c63d61e5915fcf530d96c/Progress.md
                        Directory.CreateSymbolicLink(Path.Combine(UserDirectory.DocumentsDir, AppName), CurrentPersonalPathConfigFilePath);
                    }
                    catch (Exception e)
                    {
                        DebugHelper.WriteLine("Failed to symbolic link typical ShareX path. You can safely ignore this.");
                        DebugHelper.WriteLine(e.Message);
                    }
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

        // if (ShareX.Settings.DevMode) flags.Add(nameof(ShareX.Settings.DevMode));
        if (MultiInstance) flags.Add(nameof(MultiInstance));
        if (Portable) flags.Add(nameof(Portable));
        if (SilentRun) flags.Add(nameof(SilentRun));
        if (Sandbox) flags.Add(nameof(Sandbox));
        if (IgnoreHotkeyWarning) flags.Add(nameof(IgnoreHotkeyWarning));
        // if (SystemOptions.DisableUpdateCheck) flags.Add(nameof(SystemOptions.DisableUpdateCheck));
        // if (SystemOptions.DisableUpload) flags.Add(nameof(SystemOptions.DisableUpload));
        // if (SystemOptions.DisableLogging) flags.Add(nameof(SystemOptions.DisableLogging));
        if (PuushMode) flags.Add(nameof(PuushMode));

        var output = string.Join(", ", flags);
        DebugHelper.WriteLine("Flags: " + output);
    }
}

