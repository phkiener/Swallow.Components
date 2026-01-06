using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Components.Infrastructure;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Swallow.Components.Reactive.Framework;
using Swallow.Components.Reactive.Rendering.EventHandlers;
using Swallow.Components.Reactive.Rendering.State;
using Swallow.Components.Reactive.Routing;

namespace Swallow.Components.Reactive;

/// <summary>
/// Extensions to register reactive rendering in a web host.
/// </summary>
public static class ServiceProviderConfig
{
    /// <summary>
    /// Register the services needed for reactive rendering in the given service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> in which to register the required services.</param>
    public static void AddReactiveComponents(this IServiceCollection services)
    {
        services.AddScoped<HandlerRegistration>();
        services.AddScoped<ReactiveComponentInvoker>();
        services.AddScoped<ReactiveComponentRenderer>();
        services.AddScoped<ComponentStateStore>();
        services.AddScoped<ReactiveComponentRouteResolver>();
    }

    /// <summary>
    /// Register the services needed for reactive rendering in a service collection.
    /// </summary>
    /// <param name="razorComponentsBuilder">The <see cref="IRazorComponentsBuilder"/> to use when registering the required services.</param>
    /// <seealso cref="AddReactiveComponents(IServiceCollection)"/>
    public static IRazorComponentsBuilder AddReactiveComponents(this IRazorComponentsBuilder razorComponentsBuilder)
    {
        razorComponentsBuilder.Services.AddReactiveComponents();

        return razorComponentsBuilder;
    }

    /// <summary>
    /// Map the required endpoint(s) for reactive rendering.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to which to add the endpoint(s).</param>
    /// <remarks>
    /// Only components that are marked with <see cref="ReactiveComponentAttribute"/> will be considered.
    /// </remarks>
    public static ReactiveComponentsEndpointConventionBuilder MapReactiveComponents(this IEndpointRouteBuilder endpoints)
    {
        var dataSource = endpoints.DataSources.OfType<ReactiveComponentsEndpointDataSource>().FirstOrDefault();
        if (dataSource is null)
        {
            dataSource = ActivatorUtilities.CreateInstance<ReactiveComponentsEndpointDataSource>(endpoints.ServiceProvider, endpoints);
            endpoints.DataSources.Add(dataSource);
        }

        var initialAssemblies = new[] { Assembly.GetEntryAssembly(), Assembly.GetCallingAssembly() };

        return dataSource.ConventionBuilder
            .WithStaticAssets()
            .AddAdditionalAssemblies(initialAssemblies.OfType<Assembly>());
    }

    public static RazorComponentsEndpointConventionBuilder PersistPrerenderedState(this RazorComponentsEndpointConventionBuilder builder)
    {
        // The endpoint builder for razor components ignores any endpoint filters, so... we'll have to do that dance by ourselves.
        builder.Add(eb =>
        {
            var originalDelegate = eb.RequestDelegate;
            if (originalDelegate is null)
            {
                // what.
                return;
            }

            eb.RequestDelegate = c => AppendPrerenderedStateAsync(c, originalDelegate);
        });

        return builder;
    }

    private static async Task AppendPrerenderedStateAsync(HttpContext httpContext, RequestDelegate next)
    {
        await next(httpContext);

        if (httpContext.Items.ContainsKey(ReactiveComponentBoundary.HasPrerenderedStateMarker))
        {
            var renderer = httpContext.RequestServices.GetRequiredService<IComponentPrerenderer>() as Renderer;
            var stateManager = httpContext.RequestServices.GetRequiredService<ComponentStatePersistenceManager>();

            if (renderer is null) // weird, but let's not do anything
            {
                return;
            }

            var store = new InlineStore();
            await stateManager.PersistStateAsync(store, renderer);

            var state = $"<!-- srx-prerender-state {JsonSerializer.Serialize(store.PersistedState)} -->";
            await httpContext.Response.WriteAsync(state);
        }
    }

    private sealed class InlineStore : IPersistentComponentStateStore
    {
        public IReadOnlyDictionary<string, byte[]> PersistedState { get; private set; } = new Dictionary<string, byte[]>();

        public Task<IDictionary<string, byte[]>> GetPersistedStateAsync()
        {
            return Task.FromResult<IDictionary<string, byte[]>>(new Dictionary<string, byte[]>(PersistedState));
        }

        public Task PersistStateAsync(IReadOnlyDictionary<string, byte[]> state)
        {
            PersistedState = state;
            return Task.CompletedTask;
        }

        public bool SupportsRenderMode(IComponentRenderMode renderMode)
        {
            return renderMode is null or StaticReactiveRenderMode;
        }
    }
}
