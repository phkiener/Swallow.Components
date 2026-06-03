using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Swallow.Components.Utils;

namespace Swallow.Components.Layout;

/// <summary>
/// A single tab to be displayed in a <see cref="SwTabContainer"/>.
/// </summary>
public sealed class SwTab(ElementId id) : ComponentBase, IDisposable
{
    [CascadingParameter]
    private SwTabContainer? TabContainer { get; set; }

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

    /// <summary>
    /// A callback that is invoked when the tab is displayed.
    /// </summary>
    [Parameter]
    public EventCallback<bool> OnShow { get; set; }

    /// <summary>
    /// A callback that is invoked when the tab is hidden.
    /// </summary>
    [Parameter]
    public EventCallback<bool> OnHide { get; set; }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        TabContainer?.Register(this);
    }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // We don't *directly* render anything. The container does that for us.
    }

    internal ElementId TabId { get; } = id;
    internal ElementId PanelId { get; } = new(id + "-panel");

    internal void Select(bool selected)
    {
        if (Selected == selected)
        {
            return;
        }

        Selected = selected;
        if (Selected)
        {
            _ = InvokeAsync(async () =>
            {
                await SelectedChanged.InvokeAsync(true);
                await OnShow.InvokeAsync();
            });
        }
        else
        {
            _ = InvokeAsync(async () =>
            {
                await SelectedChanged.InvokeAsync(false);
                await OnHide.InvokeAsync();
            });
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        TabContainer?.Remove(this);
    }
}
