using ShareX.Core.Task;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace ShareX.Core.Media;

public class ImageData
{
    public Stream ImageStream { get; set; }
    public EImageFormat ImageFormat { get; set; }

    public void Write(string filePath)
    {
        DebugHelper.WriteException($"ImageData.Write: {filePath}");
    }
}

public class ImageSettings
{
    public EImageFormat ImageFormat { get; set; }
    public bool ImageAutoUseJPEG { get; set; }
    public bool ImageAutoJPEGQuality { get; set; }
    public int ImageAutoUseJPEGSize { get; set; }
    public int ImageJPEGQuality { get; set; }
}

public static class ImageProcessor
{
    public static async Task<ImageData> PrepareImageAsync(Image<Rgba64> image, TaskSettings taskSettings)
    {
        var imageData = new ImageData();

        imageData.ImageStream = await SaveImageAsStreamAsync(image, taskSettings.ImageSettings.ImageFormat, taskSettings);
        imageData.ImageFormat = taskSettings.ImageSettings.ImageFormat;

        if (taskSettings.ImageSettings.ImageAutoUseJPEG && taskSettings.ImageSettings.ImageFormat != EImageFormat.JPEG &&
            imageData.ImageStream.Length > taskSettings.ImageSettings.ImageAutoUseJPEGSize * 1000)
        {
            // imageData.ImageStream.Dispose();

            if (taskSettings.ImageSettings.ImageAutoJPEGQuality)
            {
                imageData.ImageStream = await SaveJPEGAutoQualityAsync(image, taskSettings.ImageSettings.ImageAutoUseJPEGSize * 1000, 2, 70, 100);
            }
            else
            {
                imageData.ImageStream = await SaveJPEGAsync(image, taskSettings.ImageSettings.ImageJPEGQuality);
            }

            imageData.ImageFormat = EImageFormat.JPEG;
        }

        return imageData;
    }

    private static async Task<Stream> SaveImageAsStreamAsync(Image<Rgba64> image, EImageFormat format, TaskSettings taskSettings)
    {
        var memoryStream = new MemoryStream();

        switch (format)
        {
            case EImageFormat.JPEG:
                await image.SaveAsJpegAsync(memoryStream, new JpegEncoder { Quality = taskSettings.ImageSettings.ImageJPEGQuality });
                break;
            case EImageFormat.PNG:
                await image.SaveAsPngAsync(memoryStream);
                break;
            case EImageFormat.BMP:
                await image.SaveAsBmpAsync(memoryStream);
                break;
            default:
                throw new NotImplementedException($"Image format {format} not supported.");
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    private static async Task<Stream> SaveJPEGAutoQualityAsync(Image<Rgba64> image, long targetSize, int minQuality, int maxQuality, int qualityStep)
    {
        var memoryStream = new MemoryStream();
        int quality = maxQuality;

        do
        {
            await image.SaveAsJpegAsync(memoryStream, new JpegEncoder { Quality = quality });
            if (memoryStream.Length <= targetSize)
            {
                break;
            }

            quality -= qualityStep;
            memoryStream.SetLength(0);  // Reset the stream for next quality iteration

        } while (quality >= minQuality);

        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    private static async Task<Stream> SaveJPEGAsync(Image<Rgba64> image, int quality)
    {
        var memoryStream = new MemoryStream();
        await image.SaveAsJpegAsync(memoryStream, new JpegEncoder { Quality = quality });
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}
