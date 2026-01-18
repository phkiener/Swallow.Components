using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Forms;

/// <summary>
/// Represents a collection of multiple <see cref="SwFormField"/>s rendered below each other.
/// </summary>
public sealed partial class SwFieldset : ComponentBase
{
    /// <summary>
    /// The content to render.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// The orientation for each form field; vertical fields have all their parts
    /// rendered in a vertical line - which is the default - while horizontal fields
    /// will be rendered in a horizontal line.
    /// </summary>
    [Parameter]
    public Orientation Orientation { get; set; } = Orientation.Vertical;

    /// <summary>
    /// Additional HTML attributes to render onto the outermost element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object?>? AdditionalAttributes { get; set; }
}
