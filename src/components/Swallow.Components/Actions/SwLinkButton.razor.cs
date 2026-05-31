using Microsoft.AspNetCore.Components;
using Swallow.Components.Features;

namespace Swallow.Components.Actions;

/// <summary>
/// A link, styled just like an <see cref="SwButton"/>.
/// </summary>
public sealed partial class SwLinkButton : ComponentBase, IHandleAttributes
{
    /// <summary>
    /// The <c>href</c> attribute.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required string Href { get; set; }

    /// <summary>
    /// The content to display on the button.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// The <see cref="ButtonVariant"/> for this button.
    /// </summary>
    [Parameter]
    public ButtonVariant Variant { get; set; } = ButtonVariant.Default;

    /// <inheritdoc />
    [Parameter]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
