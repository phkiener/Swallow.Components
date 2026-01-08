using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Swallow.Components.Reactive.Rendering;

/// <summary>
/// The "host" component for a reactively rendered component.
/// </summary>
/// <remarks>
/// This component is not meant to be used directly.
/// </remarks>
public sealed partial class ReactiveFragment : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public required Type ComponentType { get; set; }

    [Parameter]
    [EditorRequired]
    public required IDictionary<string, object?> ComponentParameters { get; set; }

    [Parameter]
    [EditorRequired]
    public required HttpContext HttpContext { get; set; }

    protected override void OnInitialized()
    {
        if (AssignedRenderMode is not StaticReactiveRenderMode)
        {
            throw new InvalidOperationException($"{nameof(ReactiveFragment)} can only be used with static reactive rendering.");
        }
    }
}
