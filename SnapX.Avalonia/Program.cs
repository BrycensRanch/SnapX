// See https://aka.ms/new-console-template for more information
#pragma warning disable CA1416 // I am aware
using Avalonia;
using Avalonia.Dialogs;
using Avalonia.Media;
using SnapX.Avalonia;

Console.WriteLine("Initializing Dark Avalonia Core");
BuildAvaloniaApp()
    .StartWithClassicDesktopLifetime(args);

// Avalonia configuration, don't remove; also used by visual designer.
AppBuilder BuildAvaloniaApp()
   => AppBuilder.Configure<App>()
       .UsePlatformDetect()
       .UseManagedSystemDialogs()
       .WithInterFont()
       .LogToTrace()
       .With(new FontManagerOptions
       {
           DefaultFamilyName = "avares://Avalonia.Fonts.Inter/Assets#Inter"
       });
