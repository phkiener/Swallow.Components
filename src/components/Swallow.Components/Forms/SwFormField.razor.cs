using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Forms;

/// <summary>
/// A general-use form field consisting of a label, an input, a description and a validation error.
/// </summary>
public sealed partial class SwFormField : ComponentBase
{
    /// <summary>
    /// Mark this field as required, denoted by an asterisk after the <see cref="Label"/>.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// The label to render.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment Label { get; set; }

    /// <summary>
    /// The input to render.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// An additional help text to render.
    /// </summary>
    /// <remarks>
    /// This part will be hidden if there is no text content visible.
    /// </remarks>
    [Parameter]
    public RenderFragment? Description { get; set; }

    /// <summary>
    /// An optional error to display.
    /// </summary>
    /// <remarks>
    /// This part will be hidden if there is no text content visible.
    /// </remarks>
    [Parameter]
    public RenderFragment? Error { get; set; }

    /// <summary>
    /// Additional HTML attributes to render onto the outermost element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object?>? AdditionalAttributes { get; set; }
}
