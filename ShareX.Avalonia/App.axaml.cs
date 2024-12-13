using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using ShareX.Core;

namespace ShareX.Avalonia;

public class App : Application
{
    public Core.ShareX ShareX { get; set; }
    public override void Initialize()
    {

        ShareX = new Core.ShareX();
        AvaloniaXamlLoader.Load(this);

        // Default logic doesn't auto detect windows theme anymore in designer
        // to stop light mode, force here
        if (Design.IsDesignMode)
        {
            RequestedThemeVariant = ThemeVariant.Dark;
        }

    }

    public void Shutdown(object? sender, ShutdownRequestedEventArgs e)
    {
        ShareX.shutdown();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownRequested += Shutdown;
            ShareX.start(desktop.Args ?? []);
            DebugHelper.WriteLine("Internal Startup time: {0} ms", ShareX.getStartupTime());
            if (ShareX.isSilent()) return;
            var about = new AboutDialog();
            about.Show();
            DebugHelper.WriteLine($"{nameof(ShareX)}: {nameof(AboutDialog)}: {about}");

            // desktop.MainWindow = new MainWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            if (ShareX.isSilent()) return;
            // singleView.MainView = new MainView();
        }
    }
}
