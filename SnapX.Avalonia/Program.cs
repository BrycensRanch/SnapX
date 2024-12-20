// See https://aka.ms/new-console-template for more information

using Avalonia;
using SnapX.Avalonia;

Console.WriteLine("Initializing Dark Avalonia Core");
BuildAvaloniaApp()
    .StartWithClassicDesktopLifetime(args);

// Avalonia configuration, don't remove; also used by visual designer.
AppBuilder BuildAvaloniaApp()
   => AppBuilder.Configure<App>()
       .UsePlatformDetect()
       .LogToTrace();
