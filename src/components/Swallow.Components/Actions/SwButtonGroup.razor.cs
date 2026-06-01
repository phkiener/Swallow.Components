using Microsoft.AspNetCore.Components;
using Swallow.Components.Features;

namespace Swallow.Components.Actions;

/// <summary>
/// A way of coupling multiple <see cref="SwButton"/>s together.
/// </summary>
public sealed partial class SwButtonGroup : ComponentBase, IHasAdditionalAttributes
{
    /// <summary>
    /// Whether the buttons should be joined together or keep a certain distance.
    /// </summary>
    [Parameter]
    public bool Joined { get; set; }

    /// <summary>
    /// The content to display.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <inheritdoc />
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
