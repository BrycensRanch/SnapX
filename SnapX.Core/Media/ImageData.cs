using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.Media;

public class ImageData : IDisposable
{
    public Stream ImageStream { get; set; }
    public EImageFormat ImageFormat { get; set; }

    public void Write(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        ImageStream.WriteToFile(filePath);
    }
    public void Dispose()
    {
        DebugHelper.WriteLine($"ImageData.Dispose: {ImageFormat}");
        ImageStream?.Dispose();
    }
}
