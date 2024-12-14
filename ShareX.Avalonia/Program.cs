// See https://aka.ms/new-console-template for more information

using Avalonia;
using ShareX.Avalonia;

Console.WriteLine("Hello, World!");
BuildAvaloniaApp()
    .StartWithClassicDesktopLifetime(args);

// Avalonia configuration, don't remove; also used by visual designer.
AppBuilder BuildAvaloniaApp()
   => AppBuilder.Configure<App>()
       .UsePlatformDetect()
       .LogToTrace();
