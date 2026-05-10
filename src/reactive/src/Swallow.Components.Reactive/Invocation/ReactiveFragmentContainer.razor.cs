using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Reactive.Invocation;

/// <summary>
/// The container for an reactively rendered component.
/// </summary>
/// <remarks>
/// This is not meant to be used directly.
/// </remarks>
public sealed partial class ReactiveFragmentContainer : LayoutComponentBase
{
    protected override void OnParametersSet()
    {
        if (AssignedRenderMode is not StaticReactiveRenderMode)
        {
            throw new InvalidOperationException(
                $"A {nameof(ReactiveFragmentContainer)} must be rendered using {nameof(StaticReactiveRenderMode)}");
        }
    }
}
