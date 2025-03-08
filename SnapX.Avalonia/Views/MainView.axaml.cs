using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Media;
using FluentAvalonia.UI.Windowing;
using SnapX.Core;
using SnapX.Core.Upload;

namespace SnapX.Avalonia;

public partial class MainView : AppWindow
{
    public static string MainWindowName => Core.SnapX.Title + " " + Core.SnapX.VersionText;
    public bool IsPaneOpen => false;
    public MainView()
    {
        InitializeComponent();
        ListenForEvents();
    }

    public void ListenForEvents()
    {
        Core.SnapX.EventAggregator.Subscribe<NeedFileOpenerEvent>(HandleFileSelectionRequested);
    }
    private async void HandleFileSelectionRequested(NeedFileOpenerEvent @event)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = @event.Title,
            AllowMultiple = @event.Multiselect,
            SuggestedFileName = @event.FileName,
            SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(@event.Directory)
        });

        if (files.Count > 0)
        {
            string[] filePaths = files.Select(f => f.Path.ToString()).ToArray();
            UploadManager.UploadFile(filePaths, @event.TaskSettings);
        }
    }

    // Event handler for the button click
    private void OnDemoTestButtonClick(object sender, RoutedEventArgs e)
    {
        DebugHelper.WriteLine("Upload Demo Image triggered");

        // try
        // {
        //     var imageUrl = ImageURLTextBox.Text ?? ImageURLTextBox.Watermark;
        //     UploadManager.DownloadAndUploadFile(imageUrl!);
        // }
        // catch (Exception ex)
        // {
        //     DebugHelper.Logger.Error(ex.ToString());
        // }
    }
    private void ClickAboutButton(object? Sender, RoutedEventArgs E) => new AboutWindow().Show();

    private void ApplicationActualThemeVariantChanged(object? sender, EventArgs e)
    {
        if (!OperatingSystem.IsWindows()) return;
        if (IsWindows11 && ActualThemeVariant != FluentAvaloniaTheme.HighContrastTheme)
        {
            TryEnableMicaEffect();
        }
        else if (ActualThemeVariant != FluentAvaloniaTheme.HighContrastTheme)
        {
            SetValue(BackgroundProperty, AvaloniaProperty.UnsetValue);
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        Application.Current!.ActualThemeVariantChanged += ApplicationActualThemeVariantChanged;
        var thm = ActualThemeVariant;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (IsWindows11 && thm != FluentAvaloniaTheme.HighContrastTheme)
            {
                TransparencyBackgroundFallback = Brushes.Transparent;
                TransparencyLevelHint = new[]
                    { WindowTransparencyLevel.Mica, WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.None };

                TryEnableMicaEffect();
            }
        }
    }

    private void TryEnableMicaEffect()
    {
        if (ActualThemeVariant == ThemeVariant.Dark)
        {
            var color = this.TryFindResource("SolidBackgroundFillColorBase",
                ThemeVariant.Dark, out var value)
                ? (Color2)(Color)value!
                : new Color2(32, 32, 32);

            color = color.LightenPercent(-0.8f);

            Background = new ImmutableSolidColorBrush(color, 0.78);
        }
        else if (ActualThemeVariant == ThemeVariant.Light)
        {
            // Similar effect here
            var color = this.TryFindResource("SolidBackgroundFillColorBase",
                ThemeVariant.Light, out var value)
                ? (Color2)(Color)value!
                : new Color2(243, 243, 243);

            color = color.LightenPercent(0.5f);

            Background = new ImmutableSolidColorBrush(color, 0.9);
        }
    }
}
