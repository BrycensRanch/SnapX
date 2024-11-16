namespace ShareX.Uno.Presentation;

public partial class MainViewModel : ObservableObject
{
    private INavigator _navigator;

    [ObservableProperty] private string? name;

    public MainViewModel(
        INavigator navigator)
    {
        _navigator = navigator;
        Title = "Main";
        GoToSecond = new AsyncRelayCommand(GoToSecondView);
    }

    public string? Title { get; }

    public ICommand GoToSecond { get; }

    private async Task GoToSecondView()
    {
        await _navigator.NavigateViewModelAsync<SecondViewModel>(this, data: new Entity(Name!));
    }
}