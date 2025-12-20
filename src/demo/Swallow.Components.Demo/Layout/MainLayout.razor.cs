using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Demo.Layout;

public sealed partial class MainLayout : LayoutComponentBase, IDisposable
{
    [PersistentState]
    public Theme? currentTheme { get; set; } = null;

    private string? theme => currentTheme switch
    {
        Theme.Light => "light",
        Theme.Dark => "dark",
        _ => null
    };

    protected override void OnInitialized()
    {
        ThemeSelector.OnThemeChosen += OnThemeChosen;
    }

    private void OnThemeChosen(object? sender, Theme chosenTheme)
    {
        currentTheme = chosenTheme;
        StateHasChanged();
    }

    public void Dispose()
    {
        ThemeSelector.OnThemeChosen -= OnThemeChosen;
    }
}
