namespace Swallow.Components.Demo.Layout;

public enum Theme { Light, Dark }

public static class ThemeSelector
{
    public static event EventHandler<Theme>? OnThemeChosen;

    public static void Choose(Theme theme)
    {
        OnThemeChosen?.Invoke(null, theme);
    }
}
