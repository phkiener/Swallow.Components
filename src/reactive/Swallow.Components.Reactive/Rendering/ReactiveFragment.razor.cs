using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace Swallow.Components.Reactive.Rendering;

/// <summary>
/// The "host" component for a reactively rendered component.
/// </summary>
/// <remarks>
/// This component is not meant to be used directly.
/// </remarks>
public sealed partial class ReactiveFragment(IServiceProvider serviceProvider) : ComponentBase
{
    private AntiforgeryRequestToken? antiforgeryToken;

    [Parameter]
    [EditorRequired]
    public required Type ComponentType { get; set; }

    [Parameter]
    [EditorRequired]
    public required IDictionary<string, object?> ComponentParameters { get; set; }

    protected override void OnInitialized()
    {
        if (AssignedRenderMode is not StaticReactiveRenderMode)
        {
            throw new InvalidOperationException($"{nameof(ReactiveFragment)} can only be used with static reactive rendering.");
        }

        antiforgeryToken = serviceProvider.GetService<AntiforgeryStateProvider>()?.GetAntiforgeryToken();
    }
}
