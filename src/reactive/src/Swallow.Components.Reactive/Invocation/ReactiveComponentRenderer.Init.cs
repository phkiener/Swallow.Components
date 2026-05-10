using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Infrastructure;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Swallow.Components.Reactive.Invocation;

internal sealed partial class ReactiveComponentRenderer
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly MemoizingDispatcher dispatcher = new(Dispatcher.CreateDefault());

    private HttpContext? httpContext;
    private ResourceAssetCollection? resourceCollection;

    public override Dispatcher Dispatcher => dispatcher;
    protected override RendererInfo RendererInfo => new("static-interactive", true);
    protected override ResourceAssetCollection Assets => resourceCollection ?? base.Assets;

    protected override IComponentRenderMode GetComponentRenderMode(IComponent component) => RenderMode.StaticReactive;

    public async Task InitializeComponentServicesAsync(HttpContext context, IPersistentComponentStateStore store)
    {
        httpContext = context;
        resourceCollection = context.GetEndpoint()?.Metadata.GetMetadata<ResourceAssetCollection>();

        var navigationManager = serviceProvider.GetService<NavigationManager>();
        if (navigationManager is IHostEnvironmentNavigationManager hostEnvironmentNavigationManager)
        {
            var referrer = context.Request.GetTypedHeaders().Referer ??
                           throw new InvalidOperationException("Missing referer header; this endpoint should not be invoked directly.");
            var baseUri = UriHelper.BuildAbsolute(referrer.Scheme, new HostString(referrer.Host, referrer.Port), context.Request.PathBase);

            hostEnvironmentNavigationManager.Initialize(baseUri, referrer.AbsoluteUri);
        }

        var authenticationStateProvider = serviceProvider.GetService<AuthenticationStateProvider>();
        if (authenticationStateProvider is IHostEnvironmentAuthenticationStateProvider hostEnvironmentAuthenticationStateProvider)
        {
            var authenticationState = new AuthenticationState(context.User);
            hostEnvironmentAuthenticationStateProvider.SetAuthenticationState(Task.FromResult(authenticationState));
        }

        if (authenticationStateProvider is not null)
        {
            var listeners = serviceProvider.GetServices<IHostEnvironmentAuthenticationStateProvider>();
            var authenticationState = authenticationStateProvider.GetAuthenticationStateAsync();

            foreach (var listener in listeners)
            {
                listener.SetAuthenticationState(authenticationState);
            }
        }

        // TODO: Form handling.

        var stateManager = serviceProvider.GetService<ComponentStatePersistenceManager>();
        if (stateManager is not null)
        {
            stateManager.SetPlatformRenderMode(RenderMode.StaticReactive);
            await stateManager.RestoreStateAsync(store, RestoreContext.LastSnapshot);
        }
    }
}
