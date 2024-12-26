using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using SixLabors.ImageSharp;

namespace SnapX.Core.Utils.Native;

[SupportedOSPlatform("linux")]
public class LinuxAPI : NativeAPI
{
    private static bool IsWayland()
    {
        var display = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
        return !string.IsNullOrEmpty(display);
    }
    public static Rectangle GetWindowRectangle(IntPtr windowHandle)
    {
        return GetWindowRectangleX11(windowHandle);
    }
    // Importing the necessary X11 functions
    [DllImport("libX11.so")]
    private static extern IntPtr XOpenDisplay(string display);

    [DllImport("libX11.so")]
    private static extern IntPtr XRootWindow(IntPtr display, int screen_number);

    [DllImport("libX11.so")]
    private static extern IntPtr XDefaultScreenOfDisplay(IntPtr display);

    [DllImport("libX11.so")]
    private static extern void XStoreBytes(IntPtr display, IntPtr property, byte[] data, int length);

    [DllImport("libX11.so")]
    private static extern int XFlush(IntPtr display);

    [DllImport("libX11.so")]
    private static extern IntPtr XGetSelectionOwner(IntPtr display, IntPtr selection);

    [DllImport("libX11.so")]
    private static extern void XSetSelectionOwner(IntPtr display, IntPtr selection, IntPtr owner, uint time);

    // X11 Constants
    private static readonly IntPtr XA_PRIMARY = (IntPtr) 1;
    private static readonly IntPtr XA_CLIPBOARD = (IntPtr) 2;

    public override void CopyText(string text)
    {
        IntPtr display = XOpenDisplay(null);
        if (display == IntPtr.Zero)
        {
            Console.WriteLine("Unable to open X11 display.");
            return;
        }

        IntPtr rootWindow = XRootWindow(display, 0);  // Get the root window for the default screen
        IntPtr selection = XA_CLIPBOARD;

        byte[] textBytes = Encoding.UTF8.GetBytes(text);

        // Set the clipboard content by sending the data to the X server
        XSetSelectionOwner(display, selection, rootWindow, 0);
        XStoreBytes(display, selection, textBytes, textBytes.Length);
        XFlush(display);  // Ensure the data is written to the clipboard

        Console.WriteLine("Text copied to clipboard.");
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
}
