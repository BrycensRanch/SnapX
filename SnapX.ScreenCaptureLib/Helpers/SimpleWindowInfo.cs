
// SPDX-License-Identifier: GPL-3.0-or-later


using System;
using SixLabors.ImageSharp;
using SnapX.Core.Media;

namespace SnapX.ScreenCaptureLib
{
    public class SimpleWindowInfo
    {
        public IntPtr Handle { get; set; }
        public Rectangle Rectangle { get; set; }
        public bool IsWindow { get; set; }

        public WindowInfo WindowInfo
        {
            get
            {
                return new WindowInfo(Handle);
            }
        }

        public SimpleWindowInfo(IntPtr handle)
        {
            Handle = handle;
        }

        public SimpleWindowInfo(IntPtr handle, Rectangle rect)
        {
            Handle = handle;
            Rectangle = rect;
        }
    }
}
