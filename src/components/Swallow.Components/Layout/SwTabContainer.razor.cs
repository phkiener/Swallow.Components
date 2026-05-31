using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

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
        tabManager.OnSelectedTabChanged += EnqueueRender;
    }

    private void EnqueueRender(object? sender, EventArgs eventArgs)
    {
        StateHasChanged();
    }

    private async Task HandleTabClick(Tab targetTab)
    {
        foreach (var tab in tabManager.Tabs)
        {
            if (tab.Equals(targetTab))
            {
                continue;
            }

            await tab.SelectAsync(false);
        }

        await targetTab.SelectAsync(true);
    }

    private Task HandleTabInput(KeyboardEventArgs eventArgs, Tab targetTab)
    {
        if (eventArgs.Key is "Enter" or " ")
        {
            return HandleTabClick(targetTab);
        }

        return Task.CompletedTask;
    }
}
