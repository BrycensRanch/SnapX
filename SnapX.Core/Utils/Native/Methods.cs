using SixLabors.ImageSharp;
using SnapX.Core.Media;
using SnapX.Core.ScreenCapture.SharpCapture;
using SnapX.Core.ScreenCapture.SharpCapture.Linux;
using SnapX.Core.ScreenCapture.SharpCapture.macOS;
#if TARGET_WINDOWS
using SnapX.Core.ScreenCapture.SharpCapture.Windows;
#endif

namespace SnapX.Core.Utils.Native;

public static class Methods
{
    private static bool IsMacOS => OperatingSystem.IsMacOS();
    private static bool IsLinux => OperatingSystem.IsLinux();


    private static NativeAPI NativeAPI
    {
        get
        {
            #if TARGET_WINDOWS
                        return new WindowsAPI();
            #else
            if (IsMacOS) return new MacOSAPI();
            if (IsLinux) return new LinuxAPI();
            throw new PlatformNotSupportedException("This platform is not supported for native API calls.");
            #endif
        }
    }
    private static BaseCapture SharpCapture
    {
        get
        {
            #if TARGET_WINDOWS
                        return new WindowsCapture();
            #else
            if (IsMacOS) return new macOSCapture();
            if (IsLinux) return new LinuxCapture();
            throw new PlatformNotSupportedException("This platform is not supported for native API calls.");
            #endif
        }
    }
    public static List<WindowInfo> GetWindowList() => NativeAPI.GetWindowList();

    public static void ShowWindow(WindowInfo window) => NativeAPI.ShowWindow(window);
    public static void RestoreWindow(WindowInfo window) => ShowWindow(window);
    public static void CopyText(string text) => NativeAPI.CopyText(text);
    public static async Task<Image?> CaptureScreen(Screen screen) => await SharpCapture.CaptureScreen(screen);
    public static async Task<Image?> CaptureScreen(Point pos) => await SharpCapture.CaptureScreen(pos);

    public static async Task<Image?> CaptureFullscreen() => await SharpCapture.CaptureFullscreen();
    public static async Task<Image?> CaptureRectangle(Rectangle rect) => await SharpCapture.CaptureRectangle(rect);
    public static async Task<Image?> CaptureWindow(Point pos) => await SharpCapture.CaptureWindow(pos);
    public static async Task<Image?> CaptureWindow(WindowInfo window) => await SharpCapture.CaptureWindow(window);
    public static async Task<Rectangle> GetWorkingArea() => await SharpCapture.GetWorkingArea();
    public static async Task<Screen> GetPrimaryScreen() => await SharpCapture.GetPrimaryScreen();
    public static async Task<Screen> GetActiveScreen() => await SharpCapture.GetScreen(GetCursorPosition());

    public static Screen GetScreen(Point pos) => NativeAPI.GetScreen(pos);

    public static void CopyImage(Image image, string fileName) => NativeAPI.CopyImage(image, fileName);


    public static Point GetCursorPosition()
    {
        var point = Point.Empty;
        try
        {
            point = NativeAPI.GetCursorPosition();
        }
        catch (Exception ex)
        {
            DebugHelper.Logger.Warning(ex.ToString());
        }
        DebugHelper.WriteLine($"GetCursorPosition returned {point}");
        return point;
    }

    public static Rectangle GetWindowRectangle(IntPtr windowHandle = 0) =>
        NativeAPI.GetWindowRectangle(windowHandle);

    public static WindowInfo GetForegroundWindow()
    {
        // TODO: Reimplement GetForegroundWindow
        return new WindowInfo();
    }

    // Linux (Wayland): Use DBus to interact with the Wayland compositor
    private static Rectangle GetWindowRectangleWayland(IntPtr windowHandle)
    {
        // In practice, Wayland doesn't expose direct window information as X11 does.
        // You would need to use a DBus interface with the compositor (e.g., Gnome, KDE) to fetch window information.
        // This is more complex and would require integration with specific Wayland compositors.
        // For this example, we leave it unimplemented or you could integrate with dbus library.
        throw new NotImplementedException("Wayland window retrieval is not implemented.");
    }

    private static Rectangle GetWindowRectangleMacOS(IntPtr windowHandle)
    {
        throw new NotImplementedException("MacOS window retrieval is not implemented.");
    }

}

