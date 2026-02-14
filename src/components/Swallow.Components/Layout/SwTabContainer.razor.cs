using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Swallow.Components.Layout;

/// <summary>
/// A tab container that will handle multiple <see cref="SwTab"/>s while displaying
/// only a single tab.
/// </summary>
public sealed partial class SwTabContainer : ComponentBase
{
    private readonly List<SwTab> registeredTabs = [];
    private FieldIdentifier fieldIdentifier;

    /// <summary>
    /// The child content to display; should consist solely of <see cref="SwTab"/>s.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    [Parameter]
    [EditorRequired]
    public required string? ActiveTab { get; set; }

    [Parameter]
    public EventCallback<string?> ActiveTabChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public required Expression<Func<string?>> ActiveTabExpression { get; set; }

    protected override void OnParametersSet()
    {
        fieldIdentifier = FieldIdentifier.Create(ActiveTabExpression);
    }

    internal void Register(SwTab swTab)
    {
        if (!registeredTabs.Contains(swTab))
        {
            registeredTabs.Add(swTab);
            StateHasChanged();
        }
    }

    internal void Unregister(SwTab swTab)
    {
        registeredTabs.Remove(swTab);
        StateHasChanged();
    }

    private async Task SwitchTabAsync(ChangeEventArgs args)
    {
        if (args.Value is not string identifier)
        {
            return;
        }

        ActiveTab = identifier;
        await ActiveTabChanged.InvokeAsync(ActiveTab);

        StateHasChanged();
    }
}
