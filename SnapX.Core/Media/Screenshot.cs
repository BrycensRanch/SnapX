// SPDX-License-Identifier: GPL-3.0-or-later

using ScreenCapture.NET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SnapX.Core.Utils;

namespace SnapX.Core.Media;

public partial class Screenshot
{
    public bool CaptureCursor { get; set; } = false;
    public bool CaptureClientArea { get; set; } = false;
    public bool RemoveOutsideScreenArea { get; set; } = true;
    public bool CaptureShadow { get; set; } = false;
    public int ShadowOffset { get; set; } = 20;
    public bool AutoHideTaskbar { get; set; } = false;
    public IScreenCaptureService? ScreenCaptureService
    {
        get
        {
#if WINDOWS
        return new DX11ScreenCaptureService();
#elif LINUX
        return new X11ScreenCaptureService();
#else
            return null;
#endif
        }
    }


    public Image<Rgba64> CaptureRectangle(Rectangle rect)
    {
        if (RemoveOutsideScreenArea)
        {
            Rectangle bounds = CaptureHelpers.GetScreenBounds();
            rect = Rectangle.Intersect(bounds, rect);
        }

        return CaptureRectangleNative(rect, CaptureCursor);
    }

    public Image<Rgba64> CaptureFullscreen()
    {
        Rectangle bounds = CaptureHelpers.GetScreenBounds();

        return CaptureRectangle(bounds);
    }

    public Image<Rgba64> CaptureWindow(IntPtr handle)
    {
        if (handle.ToInt32() > 0)
        {
            Rectangle rect;

            if (CaptureClientArea)
            {
                // rect = Methods.GetClientRect(handle);
                throw new NotImplementedException("CaptureWindow CaptureClientArea not implemented");
            }
            else
            {
                rect = CaptureHelpers.GetWindowRectangle(handle);
            }

            bool isTaskbarHide = false;

            try
            {
                if (AutoHideTaskbar)
                {
                    // isTaskbarHide = Methods.SetTaskbarVisibilityIfIntersect(false, rect);
                    throw new NotImplementedException("AutoHideTaskbar not implemented");
                }

                return CaptureRectangle(rect);
            }
            finally
            {
                if (isTaskbarHide)
                {
                    // NativeMethods.SetTaskbarVisibility(true);
                    throw new NotImplementedException("TaskbarHide not implemented");
                }
            }
        }

        return null;
    }

    public Image<Rgba64> CaptureActiveWindow()
    {
        throw new NotImplementedException("CaptureActiveWindow not implemented");
    }

    public Image<Rgba64> CaptureActiveMonitor()
    {
        Rectangle bounds = CaptureHelpers.GetActiveScreenBounds();

        return CaptureRectangle(bounds);
    }

    private Image<Rgba64> CaptureRectangleNative(Rectangle rect, bool captureCursor = false)
    {
        // IntPtr handle = NativeMethods.GetDesktopWindow();
        // return CaptureRectangleNative(handle, rect, captureCursor);
        throw new NotImplementedException("CaptureRectangleNative not implemented");
    }

    // private Image<Rgba64> CaptureRectangleNative(IntPtr handle, Rectangle rect, bool captureCursor = false)
    // {
    //     if (rect.Width == 0 || rect.Height == 0)
    //     {
    //         return null;
    //     }
    //
    //     IntPtr hdcSrc = NativeMethods.GetWindowDC(handle);
    //     IntPtr hdcDest = NativeMethods.CreateCompatibleDC(hdcSrc);
    //     IntPtr hBitmap = NativeMethods.CreateCompatibleBitmap(hdcSrc, rect.Width, rect.Height);
    //     IntPtr hOld = NativeMethods.SelectObject(hdcDest, hBitmap);
    //     NativeMethods.BitBlt(hdcDest, 0, 0, rect.Width, rect.Height, hdcSrc, rect.X, rect.Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
    //
    //     if (captureCursor)
    //     {
    //         try
    //         {
    //             CursorData cursorData = new CursorData();
    //             cursorData.DrawCursor(hdcDest, rect.Location);
    //         }
    //         catch (Exception e)
    //         {
    //             DebugHelper.WriteException(e, "Cursor capture failed.");
    //         }
    //     }
    //
    //     NativeMethods.SelectObject(hdcDest, hOld);
    //     NativeMethods.DeleteDC(hdcDest);
    //     NativeMethods.ReleaseDC(handle, hdcSrc);
    //     Bitmap bmp = Image.FromHbitmap(hBitmap);
    //     NativeMethods.DeleteObject(hBitmap);
    //
    //     return bmp;
    // }
}

