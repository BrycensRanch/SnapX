using Gio;
using ShareX.GTK4;
using ShareX.Core.Utils;

var shareX = new ShareX.Core.ShareX();
shareX.setQualifier(" GTK4");


var application = Gtk.Application.New("ShareX.ShareX", ApplicationFlags.NonUnique);
application.OnActivate += (sender, eventArgs) =>
{
    shareX.start(args);
    var dialog = new AboutDialog();
    dialog.SetApplication(application);
    // I honestly think the bindings don't have this.
    var gtkVersion = $"Unknown, lololololl";
    var osInfo = OsInfo.GetFancyOSNameAndVersion();

    // Set system information (including GTK version)
    dialog.SystemInformation = $"OS: {osInfo}\nGTK Version: {gtkVersion}\n.NET Version: {dialog.internalAboutDialog.GetRuntime()}\nPlatform: {dialog.internalAboutDialog.GetOsPlatform()}";

    dialog.Show();
    // var window = Gtk.ApplicationWindow.New((Gtk.Application) sender);
    // window.Title = "Gtk4 Window";
    // window.SetDefaultSize(300, 300);
    // window.Show();
};
application.OnShutdown += (sender, eventArgs) =>
{
    shareX.shutdown();
};
application.Run(0, args);
