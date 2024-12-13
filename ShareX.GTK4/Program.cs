using System.Reflection;
using GdkPixbuf;
using Gio;
using GObject;
using GstVideo;
using Gtk;
using ShareX.Core.Utils;
using ShareX.Core;
using SixLabors.ImageSharp;
using AboutDialog = ShareX.GTK4.AboutDialog;
using MessageType = Gst.MessageType;

var shareX = new ShareX.Core.ShareX();
shareX.setQualifier(" GTK4");



var application = Gtk.Application.New("ShareX.ShareX", ApplicationFlags.NonUnique);
var sigintReceived = false;

Console.CancelKeyPress += (_, ea) =>
{
    ea.Cancel = true;
    sigintReceived = true;

    DebugHelper.WriteLine("Received SIGINT (Ctrl+C)");
    shareX.shutdown();
    Environment.Exit(0);
};
application.OnActivate += (sender, eventArgs) =>
{
    shareX.start(args);
    DebugHelper.WriteLine("Internal Startup time: {0} ms", shareX.getStartupTime());
    if (shareX.isSilent()) return;

    if (ShareX.Core.ShareX.CLIManager.IsCommandExist("video")) {
        Gst.Module.Initialize();
        GstVideo.Module.Initialize();
        Gst.Application.Init();
        var ret = Gst.Functions.ParseLaunch("playbin uri=playbin uri=https://ftp.nluug.nl/pub/graphics/blender/demo/movies/ToS/ToS-4k-1920.mov");
        ret.SetState(Gst.State.Playing);
        var bus = ret.GetBus();
        bus.TimedPopFiltered(Gst.Constants.CLOCK_TIME_NONE, MessageType.Eos | MessageType.Error);
        ret.SetState(Gst.State.Null);
    }

    var dialog = new AboutDialog();
    dialog.SetApplication(application);
    var gtkVersion = $"{Gtk.Functions.GetMajorVersion()}.{Gtk.Functions.GetMinorVersion()}.{Gtk.Functions.GetMicroVersion()}";
    var osInfo = OsInfo.GetFancyOSNameAndVersion();
    var assembly = Assembly.GetExecutingAssembly();
    foreach (var resourceName in assembly.GetManifestResourceNames())
    {
        DebugHelper.WriteLine(resourceName);
    }
    var bytes = assembly.ReadResourceAsByteArray("ShareX.GTK4.ShareX_Logo.png");
    var image = SixLabors.ImageSharp.Image.Load(bytes);
    using var memoryStream = new MemoryStream();
    image.SaveAsPng(memoryStream);
    var imageBytes = memoryStream.ToArray();


    var pixbuf = PixbufLoader.FromBytes(imageBytes);
    var logo =  Gdk.Texture.NewForPixbuf(pixbuf);
    // dialog.SetDecorated(false);
    // dialog.SetFocusable(false);
    // dialog.SetOpacity(0.8);
    // dialog.SetResizable(false);
    // dialog.SetCanTarget(false);
    // dialog.SetCanFocus(false);
    // dialog.SetDeletable(false);
    // dialog.SetSensitive(false);
    // dialog.SetFocusVisible(false);
    dialog.SetModal(true);
    dialog.SetLogo(logo);

    dialog.SystemInformation = $"OS: {osInfo}\nGTK Version: {gtkVersion}\n.NET Version: {dialog.internalAboutDialog.GetRuntime()}\nPlatform: {dialog.internalAboutDialog.GetOsPlatform()}";

    dialog.Show();
    // var window = Gtk.ApplicationWindow.New((Gtk.Application) sender);
    // window.Title = "Gtk4 Window";
    // window.SetDefaultSize(300, 300);
    // window.Show();
};
application.OnShutdown += (sender, eventArgs) =>
{
    sigintReceived = true;
    shareX.shutdown();
};
application.OnWindowAdded += (sender, eventArgs) =>
{
    DebugHelper.WriteLine("Actual UI Startup time: {0} ms", shareX.getStartupTime());
};
application.Run(0, args);
