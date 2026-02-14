namespace Swallow.Components.Demo.Layout.Navigation;

/// <summary>
/// A section for the navigation.
/// </summary>
/// <param name="Header">Header for this section.</param>
/// <param name="Entries">All links that are in this section.</param>
public sealed record NavigationSection(
    NavigationItem.Header Header,
    IReadOnlyList<NavigationItem.Link> Entries);

/// <summary>
/// A single item for the navigation.
/// </summary>
/// <param name="Name">The name to display for this item.</param>
/// <remarks>
/// Implemented as base class for <see cref="Header"/> and <see cref="Link"/> so that a single enumerator can cover
/// both kinds of entries. Makes it rather convenient!
/// </remarks>
public abstract record NavigationItem(string Name)
{
    public sealed record Header(string Name) : NavigationItem(Name);
    public sealed record Link(string Name, string Url) : NavigationItem(Name);
}

/// <summary>
/// Provides data for the navigation sidebar.
/// </summary>
public static class NavigationProvider
{
    /// <summary>
    /// Enumerate all sections that should be displayed in the navigation.
    /// </summary>
    /// <returns></returns>
    public static IReadOnlyList<NavigationSection> Sections => field ??= EnumerateSections().ToList();

    private static IEnumerable<NavigationItem> EnumerateItems()
    {
        yield return new NavigationItem.Header("Overview");
        yield return new NavigationItem.Link("Home", "/");
        yield return new NavigationItem.Header("Buttons");
        yield return new NavigationItem.Link("Button", Routes.Pages.Buttons.ButtonPage);
        yield return new NavigationItem.Link("Link Button", Routes.Pages.Buttons.LinkButtonPage);
        yield return new NavigationItem.Link("Icon Button", Routes.Pages.Buttons.IconButtonPage);
        yield return new NavigationItem.Header("Layout");
        yield return new NavigationItem.Link("Tabs", Routes.Pages.Layout.TabsPage);
    }

    private static IEnumerable<NavigationSection> EnumerateSections()
    {
        NavigationItem.Header? currentSection = null;
        var currentItems = new List<NavigationItem.Link>();

        foreach (var item in EnumerateItems())
        {
            switch (item)
            {
                case NavigationItem.Header header:
                {
                    if (currentSection is not null)
                    {
                        yield return new NavigationSection(currentSection, currentItems);
                    }

                    currentSection = header;
                    currentItems = [];
                    break;
                }

                case NavigationItem.Link link:
                    currentItems.Add(link);
                    break;
            }
        }

        if (currentSection is not null)
        {
            yield return new NavigationSection(currentSection, currentItems);
        }
    }
}
