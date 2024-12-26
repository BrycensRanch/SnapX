using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SnapX.Core.Media;

namespace SnapX.Core.Utils.Native;

public static class Methods
{
    private static bool IsMacOS => OperatingSystem.IsMacOS();

    private static NativeAPI NativeAPI
    {
        get
        {
#if TARGET_LINUX || LINUX
            return new LinuxAPI();
#elif WINDOWS
            return new WindowsAPI();
#else
            if (IsMacOS) return new MacOSAPI();
            throw new PlatformNotSupportedException("This platform is not supported for native API calls.");
#endif
        }
    }


    public static void ShowWindow(WindowInfo window) => NativeAPI.ShowWindow(window);
    public static void RestoreWindow(WindowInfo window) => ShowWindow(window);
    public static void CopyText(string text) => NativeAPI.CopyText(text);
    public static void CopyImage(Image image, string fileName) => NativeAPI.CopyImage(image, fileName);

    public static Rectangle GetWindowRectangle(IntPtr windowHandle = new()) =>
        NativeAPI.GetWindowRectangle(windowHandle);

    public static object GetForeground()
    {
        throw new NotImplementedException("GetForegroundWindow is not implemented");
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

