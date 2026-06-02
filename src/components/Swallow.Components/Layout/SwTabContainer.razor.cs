using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Swallow.Components.Utils;

namespace Swallow.Components.Layout;

/// <summary>
/// A container for one or more <see cref="SwTab"/>s.
/// </summary>
public sealed partial class SwTabContainer(IJSRuntime? jsRuntime = null) : ComponentBase
{
    private readonly List<SwTab> registeredTabs = [];

    /// <summary>
    /// The <see cref="SwTab"/>s to display.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    internal void Register(SwTab tab)
    {
        if (registeredTabs.Contains(tab))
        {
            return;
        }

        registeredTabs.Add(tab);

        var selectedTab = registeredTabs.FirstOrDefault(static t => t.Selected);
        if (selectedTab is null && !tab.Selected)
        {
            tab.Select(true);
        }
        else if (selectedTab is not null && tab.Selected)
        {
            selectedTab.Select(false);
        }

        StateHasChanged();
    }

    internal void Remove(SwTab tab)
    {
        var tabIndex = registeredTabs.IndexOf(tab);
        if (tabIndex is -1)
        {
            return;
        }

        if (tab.Selected)
        {
            var newSelectedTab = registeredTabs.ElementAtOrDefault(tabIndex + 1)
                                 ?? registeredTabs.ElementAtOrDefault(tabIndex - 1);

            tab.Select(false);
            newSelectedTab?.Select(true);
        }

        registeredTabs.Remove(tab);
        StateHasChanged();
    }

    private async Task OnTabListInput(KeyboardEventArgs eventArgs)
    {
        if (jsRuntime is null)
        {
            return;
        }

        var focusedElementId = await jsRuntime.GetFocusedElementIdAsync();
        var matchingTab = registeredTabs.FirstOrDefault(t => t.TabId == focusedElementId);
        if (focusedElementId is null || matchingTab is null)
        {
            return;
        }

        if (eventArgs.Key is "Enter" or " ")
        {
            var selectedTab = registeredTabs.FirstOrDefault(static t => t.Selected);
            selectedTab?.Select(false);
            matchingTab.Select(true);

            StateHasChanged();
            return;
        }

        var currentIndex = registeredTabs.IndexOfBy(t => t.TabId, focusedElementId);
        var nextIndex = eventArgs.Key switch
        {
            "Home" => 0,
            "End" => registeredTabs.Count - 1,
            "ArrowLeft" when currentIndex is 0 => registeredTabs.Count - 1,
            "ArrowLeft" => (currentIndex - 1) % registeredTabs.Count,
            "ArrowRight" => (currentIndex + 1) % registeredTabs.Count,
            _ => currentIndex
        };

        if (currentIndex != nextIndex)
        {
            var targetTab = registeredTabs.ElementAtOrDefault(nextIndex)?.TabId;
            if (targetTab is not null)
            {
                await jsRuntime.FocusElementWithIdAsync(targetTab);
            }
        }
    }

    private void OnTabClicked(SwTab targetTab)
    {
        var selectedTab = registeredTabs.FirstOrDefault(static t => t.Selected);
        selectedTab?.Select(false);

        targetTab.Select(true);
    }
}
