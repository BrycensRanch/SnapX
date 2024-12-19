using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using ShareX.Core;
using ShareX.Core.Utils;
using ShareX.Core.Utils.Native;
using SixLabors.ImageSharp.PixelFormats;

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
    private void ShowErrorDialog(string title, Exception ex)
    {

        // Create the dialog content with a button
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,


        };

        stackPanel.Children.Add(new SelectableTextBlock {
            Text = ex.Message,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(10)
        });
        stackPanel.Children.Add(new SelectableTextBlock {
            Text = ex.StackTrace,
            FontWeight = FontWeight.SemiLight,
            Padding = new Thickness(10),
        });
        stackPanel.Children.Add(new SelectableTextBlock {
            Text = GetType().Assembly.GetName().Name + ": " + GetType().Assembly.GetName().Version,
            FontWeight = FontWeight.SemiLight,
            FontSize = 16,
            FontFamily = new FontFamily("Consolas"),
            Padding = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Center,
        });

        var reportButton = new Button
        {
            Content = "Report Error",
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
            Background = Brushes.DodgerBlue,
            Foreground = Brushes.White,
            BorderBrush = Brushes.DodgerBlue,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10),
            FontWeight = FontWeight.Bold,
            CornerRadius = new CornerRadius(5)
        };
        reportButton.Click += (sender, e) => OnReportErrorClicked();

        var githubButton = new Button
        {
            Content = "Create GitHub Issue",
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
            Background = Brushes.GreenYellow,
            Foreground = Brushes.White,
            BorderBrush = Brushes.PaleGreen,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10),
            FontWeight = FontWeight.Bold,
            CornerRadius = new CornerRadius(5)
        };
        githubButton.Click += (sender, e) => OnGitHubButtonClicked(ex);


        var copyButton = new Button
        {
            Content = "Copy Error",
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0),
            Background = Brushes.Green,
            Foreground = Brushes.White,
            BorderBrush = Brushes.Green,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10),
            FontWeight = FontWeight.Bold,
            CornerRadius = new CornerRadius(5)
        };

        // Copy error to clipboard when clicked
        copyButton.Click += (sender, e) => CopyErrorToClipboard(ex.ToString());


        stackPanel.Children.Add(reportButton);
        stackPanel.Children.Add(githubButton);
        stackPanel.Children.Add(copyButton);

        // Create and show the error dialog with the formatted message
        var dialog = new Window
        {
            Title = title,
            Content = stackPanel,
            SizeToContent = SizeToContent.WidthAndHeight,
            MinWidth = 400,
            Padding = new Thickness(6),
            // Background = new ImageBrush()
            // {
            //     Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("ShareX.Avalonia.ShareX_Logo.png")!),
            //     Stretch = Stretch.UniformToFill
            // }
        };

        dialog.Show();
    }

    private void OnGitHubButtonClicked(Exception ex)
    {
        var newIssueURL = Helpers.GitHubIssueReport(ex);
        if (newIssueURL == null) return;
        URLHelpers.OpenURL(newIssueURL);
    }

    private void CopyErrorToClipboard(string errorMessage)
    {
        // Copy the error message (exception + stack trace) to the clipboard
        Clipboard.CopyText(errorMessage);
        Console.WriteLine("Error copied to clipboard.");
    }

    private void OnReportErrorClicked()
    {
        // For now, do nothing when the button is clicked
        // This is where Sentry comes in
        Console.WriteLine("Report Error button clicked. No action yet.");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var sigintReceived = false;
            desktop.ShutdownRequested += (_, _) =>
            {
                sigintReceived = true;
                DebugHelper.WriteLine("Recieved Shutdown from Avalonia");
                ShareX.shutdown();
                desktop.Shutdown();
            };

            Console.CancelKeyPress += (_, ea) =>
            {
                ea.Cancel = true;
                sigintReceived = true;

                DebugHelper.WriteLine("Received SIGINT (Ctrl+C)");
                desktop.Shutdown();
            };
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                if (!sigintReceived)
                {
                    DebugHelper.WriteLine("Received SIGTERM");
                    ShareX.shutdown();
                    desktop.Shutdown();
                }
                else
                {
                    DebugHelper.WriteLine("Received SIGTERM, ignoring it because already processed SIGINT");
                }
            };
            var errorStarting = false;
            try
            {
                ShareX.start(desktop.Args ?? []);
            }
            catch (Exception ex)
            {
                errorStarting = true;
                DebugHelper.Logger.Fatal(ex.ToString());
                ShowErrorDialog("ShareX failed to start", ex);
            }
            if (errorStarting) return;
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
