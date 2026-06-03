using Microsoft.AspNetCore.Components;
using Swallow.Components.Features;

namespace Swallow.Components.Display;

/// <summary>
/// A small badge to display a bit of data in a concise matter.
/// </summary>
public sealed partial class SwBadge : ComponentBase, IHasAdditionalAttributes
{
    /// <summary>
    /// An alternative way of setting <see cref="Label"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get => Label; set => Label = value; }

    /// <summary>
    /// The label for this badge.
    /// </summary>
    [Parameter]
    public RenderFragment? Label { get; set; }

    /// <summary>
    /// An optional value to display next to <see cref="Label"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? Value { get; set; }

    /// <inheritdoc />
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
