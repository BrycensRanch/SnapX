namespace ShareX.Core.Utils.Native;

public class Clipboard
{
    public static bool ContainsImage() => false;
    public static bool ContainsText() => false;
    public static bool ContainsFile() => false;
    public static bool ContainsData() => false;
    public static bool ContainsFileDropList => false;
    public static void CopyText(string text) => DebugHelper.WriteLine($"Clipboard.CopyText: {text}");
    public static void CopyFile(string path) => DebugHelper.WriteLine($"Clipboard.CopyFile: {path}");
    public static void Clear() => DebugHelper.WriteLine("Use your imagination to clear the clipboard.");
}
