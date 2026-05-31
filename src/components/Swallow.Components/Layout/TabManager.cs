using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Layout;

internal sealed class Tab(SwTab tab) : IEquatable<SwTab>, IEquatable<Tab>
{
    public string Id { get; } = $"tab-{Guid.NewGuid():N}";
    public bool Selected => tab.Selected;
    public RenderFragment Title => tab.Title;
    public RenderFragment Content => tab.Content;

    public Task SelectAsync(bool selected) => tab.SelectAsync(selected);

    public bool Equals(SwTab? other) => ReferenceEquals(tab, other);
    public bool Equals(Tab? other) => Id.Equals(other?.Id);
}

internal sealed class TabManager
{
    private readonly List<Tab> registeredTabs = [];

    public event EventHandler? OnTabsChanged;
    public event EventHandler? OnSelectedTabChanged;

    public IReadOnlyList<Tab> Tabs => registeredTabs.AsReadOnly();

    public void Register(SwTab tab)
    {
        tab.OnSelectedChanged += HandleSelectedTabChanged;

        registeredTabs.Add(new (tab));
        OnTabsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Unregister(SwTab tab)
    {
        tab.OnSelectedChanged -= HandleSelectedTabChanged;

        registeredTabs.Remove(new(tab));
        OnTabsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void HandleSelectedTabChanged(object? sender, EventArgs e)
    {
        OnSelectedTabChanged?.Invoke(this, EventArgs.Empty);
    }
}
