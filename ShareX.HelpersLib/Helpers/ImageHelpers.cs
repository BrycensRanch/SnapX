using SixLabors.ImageSharp;

namespace ShareX.HelpersLib;

public static class ImageHelpers {

    public static bool SaveImage(Image img, string filePath) {
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
