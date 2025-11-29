using Microsoft.AspNetCore.Components;
using Swallow.Components.Demo.Layout;

namespace Swallow.Components.Demo;

public sealed partial class App : ComponentBase, IDisposable
{
    private Theme? currentTheme = null;

    private string? theme => currentTheme switch
    {
        Theme.Light => "light",
        Theme.Dark => "dark",
        _ => null
    };

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            ThemeSelector.OnThemeChosen += OnThemeChosen;
        }
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
