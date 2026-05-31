using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Layout;

/// <summary>
/// A container for one or more <see cref="SwTab"/>s.
/// </summary>
public sealed partial class SwTabContainer : ComponentBase
{
    private readonly TabManager tabManager = new();

    /// <summary>
    /// The <see cref="SwTab"/>s to display.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    protected override void OnInitialized()
    {
        tabManager.OnTabsChanged += EnqueueRender;
    }

    private void EnqueueRender(object? sender, EventArgs eventArgs)
    {
        StateHasChanged();
    }
}
