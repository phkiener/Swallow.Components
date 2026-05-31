using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Swallow.Components.Layout;

/// <summary>
/// A single tab to be displayed in a <see cref="SwTabContainer"/>.
/// </summary>
public sealed class SwTab : ComponentBase, IDisposable
{
    [CascadingParameter]
    private TabManager? TabManager { get; set; }

    /// <summary>
    /// The title of this tab.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public RenderFragment Title { get; set; }

    /// <summary>
    /// The content to be displayed for this tab.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public RenderFragment Content { get; set; }

    /// <summary>
    /// Whether the tab is currently selected.
    /// </summary>
    [Parameter]
    public bool Selected { get; set; }

    /// <summary>
    /// A callback that is invoked when <see cref="Selected"/> changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> SelectedChanged { get; set; }

    protected override void OnInitialized()
    {
        TabManager?.Register(this);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // We don't *directly* render anything. The container does that for us.
        return;
    }

    public void Dispose()
    {
        TabManager?.Unregister(this);
    }
}
