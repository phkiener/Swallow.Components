using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure;
using Microsoft.AspNetCore.Components.Infrastructure;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swallow.Components.Reactive.Rendering;
using Swallow.Components.Reactive.Rendering.EventHandlers;
using Swallow.Components.Reactive.Rendering.State;

namespace Swallow.Components.Reactive.Framework;

internal class ReactiveComponentRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : StaticHtmlRenderer(serviceProvider, loggerFactory)
{
    private static readonly JsonSerializerOptions EventSerializationOptions = new()
    {
        MaxDepth = 32,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly IServiceProvider serviceProvider = serviceProvider;
    private ResourceAssetCollection? resourceCollection;
    private int? rootComponentId;
    private int? fragmentComponentId;

    protected override RendererInfo RendererInfo => new("static-interactive", true);

    protected override IComponentRenderMode GetComponentRenderMode(IComponent component) => RenderMode.StaticReactive;

    protected override ResourceAssetCollection Assets => resourceCollection ?? base.Assets;

    protected override ComponentState CreateComponentState(int componentId, IComponent component, ComponentState? parentComponentState)
    {
        if (parentComponentState is null)
        {
            rootComponentId = componentId;
        }

        if (parentComponentState?.ComponentId == rootComponentId && component is DynamicComponent)
        {
            fragmentComponentId = componentId;
        }

        return base.CreateComponentState(componentId, component, parentComponentState);
    }

    public async Task InitializeComponentServicesAsync(HttpContext context, ComponentStateStore store)
    {
        var navigationManager = serviceProvider.GetService<NavigationManager>();
        if (navigationManager is IHostEnvironmentNavigationManager hostEnvironmentNavigationManager)
        {
            var referrer = context.Request.GetTypedHeaders().Referer ?? throw new InvalidOperationException("Missing referer header; this endpoint should not be invoked directly.");
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

        resourceCollection = context.GetEndpoint()?.Metadata.GetMetadata<ResourceAssetCollection>();

        var stateManager = serviceProvider.GetService<ComponentStatePersistenceManager>();
        if (stateManager is not null)
        {
            stateManager.SetPlatformRenderMode(RenderMode.StaticReactive);
            await stateManager.RestoreStateAsync(store, RestoreContext.LastSnapshot);
        }
    }

    public Task RenderReactiveFragmentAsync(Type renderedComponent, IDictionary<string, object?> componentParameters)
    {
        var fragmentParameters = new Dictionary<string, object?>
        {
            [nameof(ReactiveFragment.ComponentType)] = renderedComponent,
            [nameof(ReactiveFragment.ComponentParameters)] = componentParameters,
        };

        var root = BeginRenderingComponent(typeof(ReactiveFragment), ParameterView.FromDictionary(fragmentParameters));
        return root.QuiescenceTask;
    }

    public void DiscoverEventHandlers(HandlerRegistration registration)
    {
        if (fragmentComponentId is null)
        {
            return;
        }

        ProcessPendingRender();
        registration.DiscoverEventDescriptors(fragmentComponentId.Value, GetCurrentRenderTreeFrames);
    }

    public EventArgs ParseEventArgs(ulong eventHandlerId, string? serializedArgs)
    {
        if (serializedArgs is null)
        {
            return EventArgs.Empty;
        }

        var expectedType = GetEventArgsType(eventHandlerId);
        var eventArgs = (EventArgs?)JsonSerializer.Deserialize(serializedArgs, expectedType, EventSerializationOptions) ?? EventArgs.Empty;

        return eventArgs is ChangeEventArgs { Value: JsonElement jsonValue }
            ? new ChangeEventArgs { Value = jsonValue.Deserialize<string>() }
            : eventArgs;
    }

    public void WriteHtmlTo(TextWriter output)
    {
        if (rootComponentId is null)
        {
            return;
        }

        ProcessPendingRender();
        WriteComponentHtml(rootComponentId.Value, output);
    }
}
