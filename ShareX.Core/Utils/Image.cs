using SixLabors.ImageSharp;

namespace ShareX.Core.Utils;

public static class Image {

    public static bool SaveImage(SixLabors.ImageSharp.Image img, string filePath) {
        FileHelpers.CreateDirectoryFromFilePath(filePath);

        try {
            // TODO: Implement Image Saving
            return true;
        } catch (Exception e) {
            DebugHelper.WriteException(e);
            e.ShowError();
        }

        return false;
    }
}
