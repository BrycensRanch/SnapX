using SixLabors.ImageSharp;
using SnapX.Core.Utils.Native;

namespace SnapX.Core.Media;

public class WindowInfo
{
    public WindowInfo()
    {
    }
    public int Width { get; set; } = int.MinValue;

    public int Height { get; set; } = int.MinValue;
    public Rectangle Rectangle { get; set; } = Rectangle.Empty;

    public int X { get; set; } = int.MinValue;

    public int Y { get; set; } = int.MinValue;

    public string Title { get; set; } = string.Empty;
    public IntPtr Handle { get; set; } = IntPtr.Zero;
    public WindowInfo(IntPtr windowHandle)
    {
        Handle = windowHandle;
    }

    public bool IsVisible { get; set; } = true;

    // The screen or monitor this window is currently on
    public Screen Screen { get; set; }

    public bool IsMinimized { get; set; } = false;
    public string ProcessName { get; set; } = string.Empty;

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
