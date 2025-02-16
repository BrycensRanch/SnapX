using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.PixelFormats;

namespace SnapX.Core.Utils.Native;

public class Clipboard
{
    public static bool ContainsImage() => false;

    public static bool ContainsText() => false;
    public static bool ContainsFile() => false;
    public static bool ContainsData() => false;
    public static bool ContainsFileDropList() => false;
    public static List<string> GetFileDropList() => [];
    public static Image<Rgba64> GetImage() => new(1, 1);
    public static string GetText() => string.Empty;
    public static void CopyText(string text) => Methods.CopyText(text);
    public static void CopyImage(string imagePath) => CopyImage(Image.Load(imagePath), Path.GetFileName(imagePath));
    public static void CopyImage(Image image)
    {
        var format = image.Metadata.DecodedImageFormat ?? null;

        var extension = format switch
        {
            JpegFormat _ => ".jpg",
            PngFormat _ => ".png",
            GifFormat _ => ".gif",
            BmpFormat _ => ".bmp",
            TiffFormat _ => ".tiff",
            _ => ".png"
        };
        CopyImage(image, "image" + extension);

    }

    public static void CopyImage(Image image, string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) fileName = $"image{Helpers.GetImageExtension(image)}";
        DebugHelper.WriteLine($"Clipboard.CopyImage: {image.Width}x{image.Height}): {fileName}");
        Methods.CopyImage(image, fileName);

    }

    public static void CopyFile(string path) => DebugHelper.WriteLine($"Clipboard.CopyFile: {path}");
    public static void CopyTextFromFile(string path) => DebugHelper.WriteLine($"Clipboard.CopyTextFromFile: {path}");
    public static void PasteText(string text) => DebugHelper.WriteLine($"Clipboard.PasteText: {text}");
    public static void CopyImageFromFile(string path) => DebugHelper.WriteLine($"Clipboard.CopyImageFromFile: {path}");
    public static void Clear() => DebugHelper.WriteLine("Use your imagination to clear the clipboard.");
}
