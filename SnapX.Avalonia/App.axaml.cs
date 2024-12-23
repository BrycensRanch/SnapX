using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using SnapX.Core;
using SnapX.Core.Utils;
using SnapX.Core.Utils.Native;

namespace SnapX.Avalonia;

public class App : Application
{
    public SnapX.Core.SnapX SnapX { get; set; }
    public override void Initialize()
    {

        SnapX = new SnapX.Core.SnapX();
        AvaloniaXamlLoader.Load(this);
        // for macOS
        Current!.Name = Core.SnapX.AppName;
        #if DEBUG
          Current.AttachDevTools();
        #endif

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

        stackPanel.Children.Add(new SelectableTextBlock
        {
            Text = ex.GetType() + ": " + ex.Message,
            FontWeight = FontWeight.Bold,
            Padding = new Thickness(10)
        });
        stackPanel.Children.Add(new SelectableTextBlock
        {
            Text = ex.StackTrace,
            FontWeight = FontWeight.SemiLight,
            Padding = new Thickness(10),
        });
        stackPanel.Children.Add(new SelectableTextBlock
        {
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
            //     Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("SnapX.Avalonia.SnapX_Logo.png")!),
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
                SnapX.shutdown();
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
                    SnapX.shutdown();
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
                SnapX.start(desktop.Args ?? []);
            }
            catch (Exception ex)
            {
                errorStarting = true;
                DebugHelper.Logger.Fatal(ex.ToString());
                ShowErrorDialog("SnapX failed to start", ex);
            }
            if (errorStarting) return;
            DebugHelper.WriteLine("Internal Startup time: {0} ms", SnapX.getStartupTime());
            if (SnapX.isSilent()) return;
            var about = new AboutDialog();
            about.Show();
            var demoUploadRemoteImageURL = new Window();
            demoUploadRemoteImageURL.Content = "Upload Remote Image";
            demoUploadRemoteImageURL.Width = 300;
            demoUploadRemoteImageURL.Height = 300;
            demoUploadRemoteImageURL.Background = Brushes.Gray;
            demoUploadRemoteImageURL.BorderThickness = new Thickness(1);
            demoUploadRemoteImageURL.Padding = new Thickness(5);
            demoUploadRemoteImageURL.CornerRadius = new CornerRadius(5);
            // Add a button
            var demoUploadRemoteImage = new Image();
            demoUploadRemoteImage.Width = 100;
            demoUploadRemoteImage.Height = 100;

            DebugHelper.WriteLine($"{nameof(SnapX)}: {nameof(AboutDialog)}: {about}");

            // desktop.MainWindow = new MainWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            if (SnapX.isSilent()) return;
            // singleView.MainView = new MainView();
        }
    }
}
