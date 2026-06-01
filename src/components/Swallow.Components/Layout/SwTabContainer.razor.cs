using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Swallow.Components.Layout;

/// <summary>
/// A container for one or more <see cref="SwTab"/>s.
/// </summary>
public sealed partial class SwTabContainer(IJSRuntime? jsRuntime = null) : ComponentBase
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

    private async Task HandleTabListInput(KeyboardEventArgs eventArgs)
    {
        if (jsRuntime is null)
        {
            return;
        }

        var focusedElement = await jsRuntime.GetValueAsync<IJSObjectReference>("document.activeElement");
        var id = await focusedElement.GetValueAsync<string?>("id");

        var matchingTab = tabManager.Tabs.FirstOrDefault(t => t.Id + "-handle" == id);
        if (matchingTab is null)
        {
            return;
        }

        var currentIndex = tabManager.Tabs.Index().SingleOrDefault(t => t.Item == matchingTab).Index;
        var nextIndex = eventArgs.Key switch
        {
            "Home" => 0,
            "End" => tabManager.Tabs.Count - 1,
            "ArrowLeft" when currentIndex is 0 => tabManager.Tabs.Count - 1,
            "ArrowLeft" => (currentIndex - 1) % tabManager.Tabs.Count,
            "ArrowRight" => (currentIndex + 1) % tabManager.Tabs.Count,
            _ => currentIndex
        };

        if (currentIndex != nextIndex)
        {
            var targetTab = tabManager.Tabs[nextIndex].Id;
            var targetElement = await jsRuntime.InvokeAsync<IJSObjectReference>("document.getElementById", [targetTab + "-handle"]);
            await targetElement.InvokeVoidAsync("focus", new { focusVisible = true });
        }
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
