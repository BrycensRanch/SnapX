using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace ShareX.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        var sharex = new Core.ShareX();
        sharex.start();
        var about = new AboutDialog();
        about.Show();

        // AvaloniaXamlLoader.Load(this);

        // Default logic doesn't auto detect windows theme anymore in designer
        // to stop light mode, force here
        if (Design.IsDesignMode)
        {
            RequestedThemeVariant = ThemeVariant.Dark;
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        // {
        //     desktop.MainWindow = new MainWindow();
        // }
        // else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        // {
        //     singleView.MainView = new MainView();
        // }
    }
}
