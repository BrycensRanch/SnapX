using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SnapX.Core.Media;
using SnapX.Core.Utils.Native;
using Tmds.DBus;
using Tmds.DBus.Protocol;

namespace SnapX.Core.ScreenCapture.SharpCapture.Linux;
public class LinuxCapture : BaseCapture
{
    public override async Task<Image> CaptureFullscreen()
    {
        if (LinuxAPI.IsWayland()) return await TakeScreenshotWithPortal();
        return await TakeScreenshotWithPortal();
    }

    private static async Task<Image> TakeScreenshotWithPortal()
    {
        var connection = new Connection(Address.Session);
        await connection.ConnectAsync().ConfigureAwait(false);
        var desktop = new DesktopService(connection, "org.freedesktop.portal.Desktop");
        // var access = new DesktopService(connection, "org.freedesktop.access");
        var screenshot = desktop.CreateScreenshot("/org/freedesktop/portal/desktop");
        var options = new Dictionary<string, VariantValue>()
        {
            // { "interactive", true }
        };
        var timeoutTask = Task.Delay(10000);
        var portalResponse = connection.Call(() => screenshot.ScreenshotAsync("", options));

        var completedTask = await Task.WhenAny(portalResponse, timeoutTask);

        if (completedTask == timeoutTask)
        {
            throw new TimeoutException("Call to org.freedesktop.portal.Desktop Screenshot timed out. Please try again.");
        }
        var Response = await portalResponse;
        var uri = new Uri(Response.Results["uri"].GetString());
        var fileURL = Uri.UnescapeDataString(uri.LocalPath);
        var img = await Image.LoadAsync(fileURL);
        _ = Task.Run(() =>
        {
            try
            {
                File.Delete(fileURL);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteException(ex);
            }
        });

        return img;
    }
    private static Image CropFullscreenScreenshotToScreen(Screen screen, Image img)
    {
        var bounds = screen.Bounds;
        var cropRectangle = new Rectangle(
            Math.Max(0, bounds.X),
            Math.Max(0, bounds.Y),
            Math.Min(img.Width - bounds.X, bounds.Width),
            Math.Min(img.Height - bounds.Y, bounds.Height)
        );

        img.Mutate(x => x.Crop(cropRectangle));

        return img;
    }
    public override async Task<Image> CaptureScreen(Screen screen)
    {
        // TODO: Implement pure X11 screenshotting instead of using portal
        // if (LinuxAPI.IsWayland())
        // {
            var syncContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);


            var fullscreenImage = await TakeScreenshotWithPortal().ConfigureAwait(false);
            Console.WriteLine($"{fullscreenImage.Width}x{fullscreenImage.Height} {fullscreenImage.Configuration.ImageFormats}");
            var croppedImage = CropFullscreenScreenshotToScreen(screen, fullscreenImage);
            Console.WriteLine($"{croppedImage.Width}x{croppedImage.Height} {croppedImage.Configuration.ImageFormats}");
            SynchronizationContext.SetSynchronizationContext(syncContext);
            return croppedImage;
        // }

        // return LinuxAPI.TakeScreenshotWithX11(screen);
    }
}
