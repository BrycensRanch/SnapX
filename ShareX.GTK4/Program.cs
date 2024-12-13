using Gio;
using ShareX.GTK4;
using ShareX.Core.Utils;
using ShareX.Core;

var shareX = new ShareX.Core.ShareX();
shareX.setQualifier(" GTK4");



var application = Gtk.Application.New("ShareX.ShareX", ApplicationFlags.NonUnique);
application.OnActivate += (sender, eventArgs) =>
{
    shareX.start(args);
    DebugHelper.WriteLine("Internal Startup time: {0} ms", shareX.getStartupTime());
    if (shareX.isSilent()) return;

    var dialog = new AboutDialog();
    dialog.SetApplication(application);
    var gtkVersion = $"{Gtk.Functions.GetMajorVersion()}.{Gtk.Functions.GetMinorVersion()}.{Gtk.Functions.GetMicroVersion()}";
    var osInfo = OsInfo.GetFancyOSNameAndVersion();

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
application.OnWindowAdded += (sender, eventArgs) =>
{
    DebugHelper.WriteLine("Actual UI Startup time: {0} ms", shareX.getStartupTime());
};
application.Run(0, args);
