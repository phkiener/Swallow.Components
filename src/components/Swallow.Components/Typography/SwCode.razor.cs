using Microsoft.AspNetCore.Components;
using Swallow.Components.Features;

namespace Swallow.Components.Typography;

/// <summary>
/// An inline code snippet.
/// </summary>
public sealed partial class SwCode : ComponentBase, IHasAdditionalAttributes
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
