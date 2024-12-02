using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;

namespace ShareX.Avalonia.Services;

public class NavigationService
{
    public static NavigationService Instance { get; } = new NavigationService();

    public Control PreviousPage { get; set; }

    public void SetFrame(Frame f)
    {
        _frame = f;
    }

    public void SetOverlayHost(Panel p)
    {
        _overlayHost = p;
    }

    public void Navigate(Type t)
    {
        _frame.Navigate(t);
    }

    public void NavigateFromContext(object dataContext, NavigationTransitionInfo transitionInfo = null)
    {
        _frame.NavigateFromObject(dataContext,
            new FluentAvalonia.UI.Navigation.FrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = transitionInfo ?? new SuppressNavigationTransitionInfo()
            });
    }

    public void ShowControlDefinitionOverlay(Type targetType)
    {
    }

    public void ClearOverlay()
    {
        _overlayHost?.Children.Clear();

    }

    private Frame _frame;
    private Panel _overlayHost;
}


