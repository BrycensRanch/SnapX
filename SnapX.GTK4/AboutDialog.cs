using System.Reflection;
using GdkPixbuf;
using GObject;
using SnapX.Core;

namespace SnapX.GTK4;

public class AboutDialog : Gtk.AboutDialog
{
    public CommonUI.AboutDialog internalAboutDialog = new CommonUI.AboutDialog();
    public AboutDialog()
    {

        Comments = internalAboutDialog.GetDescription();
        Copyright = internalAboutDialog.GetCopyright();
        License = internalAboutDialog.GetLicense();
        Logo = LoadFromResource("SnapX.GTK4.SnapX_Logo.png");
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
            DebugHelper.WriteLine($"Unable to load image resource '{resourceName}': {e.ToString()}");
            return null;
        }
    }
}
