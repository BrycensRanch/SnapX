using System.Reflection;
using GdkPixbuf;
using GObject;
namespace ShareX.GTK4;

public class AboutDialog : Gtk.AboutDialog
{
    public AboutDialog(string sampleName)
    {
        Comments = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
        Copyright = "© BrycensRanch & ShareX Team 2024-present";
        License = "AGPL v3 or Later";
        Logo = LoadFromResource("ShareX.GTK4.logo.svg");
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        Website = "https://github.com/BrycensRanch/ShareX-Linux-Port";
        LicenseType = Gtk.License.Agpl30;
        ProgramName = $"{sampleName} - Powered by GirCore";
    }
    private static Gdk.Texture? LoadFromResource(string resourceName)
    {
        try
        {
            var bytes = Assembly.GetExecutingAssembly().ReadResourceAsByteArray(resourceName);
            var pixbuf = PixbufLoader.FromBytes(bytes);
            return Gdk.Texture.NewForPixbuf(pixbuf);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unable to load image resource '{resourceName}': {e.Message}");
            return null;
        }
    }
}
