using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.Media;

public class ImageData : IDisposable
{
    public Stream ImageStream { get; set; }
    public EImageFormat ImageFormat { get; set; }

    public void Write(string filePath)
    {
        const int maxRetries = 5;
        const int retryDelayMilliseconds = 1000; // 1 second
        int retryCount = 0;
        bool fileSaved = false;

        while (retryCount < maxRetries && !fileSaved)
        {
            try
            {
                // Attempt to save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    ImageStream.CopyTo(fileStream); // Copy the stream to the file
                }

                // If no exception was thrown, file is saved successfully
                DebugHelper.WriteLine($"File saved successfully to {filePath}.");
                fileSaved = true;
            }
            catch (IOException ex)
            {
                retryCount++;

                // Log the exception message and retry information
                DebugHelper.WriteLine(
                    $"Attempt {retryCount} failed. IOException: {ex.Message}. Retrying in {retryDelayMilliseconds / 1000} second(s)...");

                if (retryCount < maxRetries)
                {
                    // Wait for the specified delay before retrying
                    Thread.Sleep(retryDelayMilliseconds);
                }
                else
                {
                    // If max retries reached, log failure
                    DebugHelper.WriteLine($"Failed to save the file after {maxRetries} retries.");
                }
            }
        }
    }
    public void Dispose()
    {
        DebugHelper.Logger?.Debug($"ImageData.Dispose: {ImageFormat}");
        ImageStream?.Dispose();
    }

}
