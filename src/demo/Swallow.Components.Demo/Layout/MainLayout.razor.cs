using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Demo.Layout;

public sealed partial class MainLayout(NavigationManager navigationManager) : LayoutComponentBase
{
    private readonly List<SectionHeader> sections = [];

    internal void Register(SectionHeader header)
    {
        if (sections.Contains(header))
        {
            return;
        }

        sections.Add(header);
        StateHasChanged();
    }

    internal void Remove(SectionHeader header)
    {
        if (!sections.Contains(header))
        {
            return;
        }

        sections.Remove(header);
        StateHasChanged();
    }
}
