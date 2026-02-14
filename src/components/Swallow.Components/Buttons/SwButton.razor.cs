using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Swallow.Components.Buttons;

/// <summary>
/// A plain button.
/// </summary>
public sealed partial class SwButton : SwButtonBase
{
    private string buttonType => OnClick.HasDelegate ? "button" : "submit";

    /// <summary>
    /// The content for the button.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Callback to invoke when clicking the button.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }
}
