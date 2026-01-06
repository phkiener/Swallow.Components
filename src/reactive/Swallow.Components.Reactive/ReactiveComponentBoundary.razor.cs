using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Swallow.Components.Reactive.Routing;

namespace Swallow.Components.Reactive;

/// <summary>
/// A render-mode boundary to switch from static rendering to reactive rendering.
/// </summary>
public sealed partial class ReactiveComponentBoundary(NavigationManager navigationManager, IServiceProvider serviceProvider) : ComponentBase
{
    internal const string HasPrerenderedStateMarker = "_srx-prerender-state";

    private string? targetUrl;
    private AntiforgeryRequestToken? antiforgeryToken;

    /// <summary>
    /// The component to render reactively; needs to have the
    /// <see cref="ReactiveComponentAttribute"/>.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required Type ComponentType { get; set; }

    /// <summary>
    /// The parameters to pass to the rendered component.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public IDictionary<string, object?> ComponentParameters { get; set; }

    [Parameter]
    public bool Prerender { get; set; } = false;

    [CascadingParameter]
    public HttpContext? HttpContext { get; set; }

    [Inject]
    private ReactiveComponentRouteResolver RouteResolver { get; set; } = null!;


    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (AssignedRenderMode is not null)
        {
            throw new InvalidOperationException($"{nameof(ReactiveComponentBoundary)} can only be used while rendering statically.");
        }

        antiforgeryToken = serviceProvider.GetService<AntiforgeryStateProvider>()?.GetAntiforgeryToken();
        HttpContext?.Items.TryAdd(HasPrerenderedStateMarker, true);
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        var route = RouteResolver.ResolveRoute(ComponentType) ?? throw NoEndpointFound(ComponentType);
        targetUrl = navigationManager.ToAbsoluteUri(route).AbsolutePath;
    }

    private static ArgumentException NoEndpointFound(Type componentType)
    {
        return new ArgumentException(
            $"No route found for {componentType}; check that the type does have the {nameof(ReactiveComponentAttribute)} and {nameof(ServiceProviderConfig.MapReactiveComponents)} has been called.");
    }
}
