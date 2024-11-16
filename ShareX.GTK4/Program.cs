using System.Diagnostics;
using System.Reflection;
using Gio;
using ShareX.GTK4;
using ShareX.Core;
using ShareX.Core.Helpers;
using System.Runtime.InteropServices;


var application = Gtk.Application.New("ShareX.ShareX", ApplicationFlags.NonUnique);
        application.OnActivate += (sender, args) =>
        {
            ShareX.Core.ShareX shareX = new ShareX.Core.ShareX();

            shareX.start();
            Console.WriteLine(args);
            var dialog = new AboutDialog("ShareX on GTK4");
            dialog.SetApplication(application);
            // I honestly think the bindings don't have this.
            string gtkVersion = $"Unknown, lololololl";
            string osInfo = OsInfo.GetFancyOSNameAndVersion();

            // Set system information (including GTK version)
            dialog.SystemInformation = $"OS: {osInfo}\nGTK Version: {gtkVersion}\n.NET Version: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}\nPlatform: {Environment.OSVersion.Platform} {Environment.OSVersion.Version}";

            dialog.Show();
            // var window = Gtk.ApplicationWindow.New((Gtk.Application) sender);
            // window.Title = "Gtk4 Window";
            // window.SetDefaultSize(300, 300);
            // window.Show();
        };
        application.Run(0, args);
        // return application.RunWithSynchronizationContext(null);
