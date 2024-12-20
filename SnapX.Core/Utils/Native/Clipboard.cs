using System.Collections.Specialized;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SnapX.Core.Utils.Native;

public class Clipboard
{
    public static bool ContainsImage() => false;
    public static bool ContainsText() => false;
    public static bool ContainsFile() => false;
    public static bool ContainsData() => false;
    public static bool ContainsFileDropList() => false;
    public static StringCollection GetFileDropList() => new();
    public static SixLabors.ImageSharp.Image<Rgba64> GetImage() => new Image<Rgba64>(1, 1);
    public static string GetText() => string.Empty;
    public static void CopyText(string text) => DebugHelper.WriteLine($"Clipboard.CopyText: {text}");
    public static void CopyImage(string imagePath) => DebugHelper.WriteLine($"Clipboard.CopyImage: {imagePath}");
    public static void CopyImage(Image<Rgba64> image, string fileName) => DebugHelper.WriteLine($"Clipboard.CopyImage: {image.Width}x{image.Height}): {fileName}");

    public static void CopyFile(string path) => DebugHelper.WriteLine($"Clipboard.CopyFile: {path}");
    public static void CopyTextFromFile(string path) => DebugHelper.WriteLine($"Clipboard.CopyTextFromFile: {path}");
    public static void PasteText(string text) => DebugHelper.WriteLine($"Clipboard.PasteText: {text}");
    public static void CopyImageFromFile(string path) => DebugHelper.WriteLine($"Clipboard.CopyImageFromFile: {path}");
    public static void Clear() => DebugHelper.WriteLine("Use your imagination to clear the clipboard.");
}
