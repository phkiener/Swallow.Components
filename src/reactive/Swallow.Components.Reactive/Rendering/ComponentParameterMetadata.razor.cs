using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Reactive.Rendering;

/// <summary>
/// Persists parameters for a component as <c><![CDATA[<meta>]]></c> tags.
/// </summary>
/// <remarks>
/// This component is not meant to be used directly.
/// </remarks>
public sealed partial class ComponentParameterMetadata : ComponentBase
{
    /// <summary>
    /// The parameters to persist.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required IReadOnlyDictionary<string, object> Parameters { get; set; }
}
