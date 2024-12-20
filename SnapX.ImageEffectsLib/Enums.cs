
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;

namespace ShareX.ImageEffectsLib
{
    public enum WatermarkType
    {
        Text,
        Image
    }

    public enum ResizeMode
    {
        [Description("Resizes all images to the specified size.")]
        ResizeAll,
        [Description("Only resize image if it is bigger than specified size.")]
        ResizeIfBigger,
        [Description("Only resize image if it is smaller than specified size.")]
        ResizeIfSmaller
    }

    public enum DrawImageSizeMode // Localized
    {
        DontResize,
        AbsoluteSize,
        PercentageOfWatermark,
        PercentageOfCanvas
    }

    public enum ImageRotateFlipType
    {
        None = 0,
        Rotate90 = 1,
        Rotate180 = 2,
        Rotate270 = 3,
        FlipX = 4,
        Rotate90FlipX = 5,
        FlipY = 6,
        Rotate90FlipY = 7
    }
}