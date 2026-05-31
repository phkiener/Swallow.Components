using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Layout;

internal sealed class Tab(SwTab tab) : IEquatable<SwTab>
{
    public string Id { get; } = $"tab-{Guid.NewGuid():N}";
    public bool Selected => tab.Selected;
    public RenderFragment Title => tab.Title;
    public RenderFragment Content => tab.Content;

    public bool Equals(SwTab? other) => ReferenceEquals(tab, other);
}

internal sealed class TabManager
{
    private readonly List<Tab> registeredTabs = [];

    public event EventHandler? OnTabsChanged;
    public IReadOnlyList<Tab> Tabs => registeredTabs.AsReadOnly();

    public void Register(SwTab tab)
    {
        registeredTabs.Add(new (tab));
        OnTabsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Unregister(SwTab tab)
    {
        registeredTabs.Remove(new(tab));
        OnTabsChanged?.Invoke(this, EventArgs.Empty);
    }
}
