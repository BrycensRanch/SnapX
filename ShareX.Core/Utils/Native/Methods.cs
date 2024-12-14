using System.Runtime.InteropServices;
using SixLabors.ImageSharp;

namespace ShareX.Core.Utils.Native;

public class Methods
{
    public static Rectangle GetWindowRectangle(IntPtr windowHandle)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowRectangleWindows(windowHandle);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (IsWayland())
            {
                return GetWindowRectangleWayland(windowHandle);
            }
            else
            {
                return GetWindowRectangleX11(windowHandle);
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetWindowRectangleMacOS(windowHandle);
        }
        else
        {
            throw new PlatformNotSupportedException($"Unsupported platform {RuntimeInformation.OSDescription}");
        }
    }

    public static object GetForeground()
    {
        throw new NotImplementedException("GetForegroundWindow is not implemented");
    }
    private static Rectangle GetWindowRectangleWindows(IntPtr windowHandle)
    {
        GetWindowRect(windowHandle, out RECT rect);
        return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }


    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    // Linux (X11): Use X11's XGetGeometry
    private static Rectangle GetWindowRectangleX11(IntPtr windowHandle)
    {
        IntPtr display = XOpenDisplay(null);
        if (display == IntPtr.Zero)
            throw new InvalidOperationException("Unable to open X11 display.");

        XWindowAttributes attributes = new XWindowAttributes();
        if (XGetWindowAttributes(display, windowHandle, ref attributes) != 0)
        {
            return new Rectangle(attributes.x, attributes.y, attributes.width, attributes.height);
        }

        throw new InvalidOperationException("Unable to get window attributes.");
    }

    [DllImport("libX11.so")]
    private static extern IntPtr XOpenDisplay(string display);

    [DllImport("libX11.so")]
    private static extern int XGetWindowAttributes(IntPtr display, IntPtr window, ref XWindowAttributes attributes);

    [StructLayout(LayoutKind.Sequential)]
    public struct XWindowAttributes
    {
        public int x, y;
        public int width, height;
        public int border_width, depth;
        public IntPtr visual;
        public IntPtr colormap;
        public int class_type;
        public IntPtr root;
    }

    private static bool IsWayland()
    {
        string display = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
        return !string.IsNullOrEmpty(display);
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

