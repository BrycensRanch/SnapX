using System;
using System.IO;
using SixLabors.ImageSharp;

namespace ShareX.HelpersLib;

public static class ImageHelpers
{


    public static MemoryStream SaveGIF(Image img, GIFQuality quality)
    {
        MemoryStream ms = new MemoryStream();
        SaveGIF(img, ms, quality);
        return ms;
    }

        public static ImageFormat GetImageFormat(string filePath)
        {
            ImageFormat imageFormat = ImageFormat.Png;
            string ext = FileHelpers.GetFileNameExtension(filePath);

            if (!string.IsNullOrEmpty(ext))
            {
                if (ext.Equals("png", StringComparison.OrdinalIgnoreCase))
                {
                    imageFormat = ImageFormat.Png;
                }
                else if (ext.Equals("jpg", StringComparison.OrdinalIgnoreCase) || ext.Equals("jpeg", StringComparison.OrdinalIgnoreCase) ||
                    ext.Equals("jpe", StringComparison.OrdinalIgnoreCase) || ext.Equals("jfif", StringComparison.OrdinalIgnoreCase))
                {
                    imageFormat = ImageFormat.Jpeg;
                }
                else if (ext.Equals("gif", StringComparison.OrdinalIgnoreCase))
                {
                    imageFormat = ImageFormat.Gif;
                }
                else if (ext.Equals("bmp", StringComparison.OrdinalIgnoreCase))
                {
                    imageFormat = ImageFormat.Bmp;
                }
                else if (ext.Equals("tif", StringComparison.OrdinalIgnoreCase) || ext.Equals("tiff", StringComparison.OrdinalIgnoreCase))
                {
                    imageFormat = ImageFormat.Tiff;
                }
            }

            return imageFormat;
        }

        public static bool SaveImage(Image img, string filePath)
        {
            FileHelpers.CreateDirectoryFromFilePath(filePath);
            ImageFormat imageFormat = GetImageFormat(filePath);

            try
            {
                img.Save(filePath, imageFormat);
                return true;
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
                e.ShowError();
            }

            return false;
        }
    public static void SaveGIF(Image img, Stream stream, GIFQuality quality)
    {
        if (quality == GIFQuality.Default)
        {
            img.Save(stream, ImageFormat.Gif);
        }
        else
        {
            Quantizer quantizer;

            switch (quality)
            {
                case GIFQuality.Grayscale:
                    quantizer = new GrayscaleQuantizer();
                    break;
                case GIFQuality.Bit4:
                    quantizer = new OctreeQuantizer(15, 4);
                    break;
                default:
                case GIFQuality.Bit8:
                    quantizer = new OctreeQuantizer(255, 4);
                    break;
            }

            using (Bitmap quantized = quantizer.Quantize(img))
            {
                quantized.Save(stream, ImageFormat.Gif);
            }
        }
    }
}
