using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using SixLabors.ImageSharp;
using SnapX.Core.Media;

namespace SnapX.Core.Utils.Native;

public class WindowsAPI : NativeAPI
{
    // Constants for Windows semantics
    private const int SW_HIDE = 0;       // Hide the window
    private const int SW_SHOW = 5;       // Show the window
    private const int SW_MINIMIZE = 6;   // Minimize the window
    private const int SW_RESTORE = 9;    // Restore the window
    private const int SW_SHOWDEFAULT = 10;
    private const int RetryTimes = 20;
    private const int RetryDelay = 100;
    // Constants for allocating memory and setting data format
    public const uint CF_TEXT = 1;
    public const int GMEM_ZEROINIT = 0x0040;

    private static readonly object ClipboardLock = new();


    public override void ShowWindow(WindowInfo Window)
    {
        var handle = Window.Handle;
        if (handle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Invalid window handle.");
        }
        ShowWindow(handle, SW_SHOW);

    }
    public override void ShowWindow(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Invalid window handle.");
        }
        ShowWindow(handle, SW_SHOW);

    }

    public override void CopyText(string text)
    {
        if (!OpenClipboard(IntPtr.Zero))
        {
            throw new AccessViolationException("Failed to open clipboard.");
        }

        // Empty the clipboard
        EmptyClipboard();

        // Allocate global memory for the text
        IntPtr hGlobal = GlobalAlloc(GMEM_ZEROINIT, (uint)(text.Length + 1) * sizeof(char));

        // Lock the memory so that we can copy the text into it
        IntPtr lpGlobal = GlobalLock(hGlobal);

        // Copy the text into the allocated memory
        Marshal.Copy(text.ToCharArray(), 0, lpGlobal, text.Length);

        // Unlock the memory
        GlobalUnlock(hGlobal);

        // Set the clipboard data (CF_TEXT is used for plain text)
        SetClipboardData(CF_TEXT, hGlobal);

        // Close the clipboard
        CloseClipboard();

        Console.WriteLine("Text copied to clipboard.");
    }
    // Import the necessary Windows API functions via DllImport
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    private static extern bool EmptyClipboard();

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GlobalAlloc(int uFlags, uint dwBytes);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
    public override Rectangle GetWindowRectangle(WindowInfo Window)
    {
        var handle = Window.Handle;
        if (handle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Invalid window handle.");
        }
        GetWindowRect(handle, out RECT rect);
        return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }
    public static Rectangle GetWindowRect(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Invalid window handle.");
        }
        GetWindowRect(handle, out RECT rect);
        return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }
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
}
