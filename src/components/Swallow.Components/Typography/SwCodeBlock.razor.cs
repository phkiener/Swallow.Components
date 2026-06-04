using Microsoft.AspNetCore.Components;
using Swallow.Components.Features;

namespace Swallow.Components.Typography;

/// <summary>
/// A clode block.
/// </summary>
public sealed partial class SwCodeBlock : ComponentBase, IHasAdditionalAttributes
{
    /// <summary>
    /// The content to display.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
