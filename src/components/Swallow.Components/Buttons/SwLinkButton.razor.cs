using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Buttons;

/// <summary>
/// A link that is styled as button.
/// </summary>
public sealed partial class SwLinkButton : SwButtonBase
{
    /// <summary>
    /// The content for the button.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// The link to render.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required string Href { get; set; }
}
