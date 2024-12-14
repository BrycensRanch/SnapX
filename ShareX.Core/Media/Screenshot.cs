#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2024 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)


using ShareX.Core.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ShareX.Core.Media
{
    public partial class Screenshot
    {
        public bool CaptureCursor { get; set; } = false;
        public bool CaptureClientArea { get; set; } = false;
        public bool RemoveOutsideScreenArea { get; set; } = true;
        public bool CaptureShadow { get; set; } = false;
        public int ShadowOffset { get; set; } = 20;
        public bool AutoHideTaskbar { get; set; } = false;

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
}
