using System.Reflection;
using GdkPixbuf;
using GObject;
namespace ShareX.GTK4;

public class AboutDialog : Gtk.AboutDialog
{
    public ShareX.CommonUI.AboutDialog internalAboutDialog = new CommonUI.AboutDialog();
    public AboutDialog()
    {

        Comments = internalAboutDialog.GetDescription();
        Copyright = internalAboutDialog.GetCopyright();
        License = internalAboutDialog.GetLicense();
        Logo = LoadFromResource("ShareX.GTK4.logo.svg");
        Version = internalAboutDialog.GetVersion();
        Website = internalAboutDialog.GetWebsite();
        LicenseType = Gtk.License.Gpl30;
        ProgramName = $"{internalAboutDialog.GetTitle()}";
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
