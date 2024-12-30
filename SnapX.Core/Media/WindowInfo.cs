using SixLabors.ImageSharp;
using SnapX.Core.Utils.Native;

namespace SnapX.Core.Media;

public class WindowInfo
{
    public WindowInfo()
    {
    }
    public static int Width { get; set; } = int.MinValue;

    public static int Height { get; set; } = int.MinValue;
    public Rectangle Rectangle { get; set; } = new(X, Y, Width, Height);

    public static int X { get; set; } = int.MinValue;

    public static int Y { get; set; } = int.MinValue;

    public string Title { get; set; } = string.Empty;
    public IntPtr Handle { get; set; } = IntPtr.Zero;
    public WindowInfo(IntPtr windowHandle)
    {
        Handle = windowHandle;
    }

    public bool IsVisible { get; set; } = true;

    public double Opacity { get; set; } = 1.0;

    // The screen or monitor this window is currently on
    public Screen Screen { get; set; }

    public bool IsMinimized { get; set; } = false;

    public bool IsActive { get; set; } = false;
    public virtual void Restore()
    {
        Methods.RestoreWindow(this);
    }

    public virtual void Activate()
    {
        IsActive = true;
        Methods.ShowWindow(this);
    }

}
