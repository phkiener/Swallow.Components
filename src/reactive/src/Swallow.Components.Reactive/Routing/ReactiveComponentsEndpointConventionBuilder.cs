using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Swallow.Components.Reactive.Routing;

/// <summary>
/// An <see cref="IEndpointConventionBuilder" /> for reactive components.
/// </summary>
public sealed class ReactiveComponentsEndpointConventionBuilder : IEndpointConventionBuilder
{
    private readonly Lock lockObject;
    private readonly ReactiveComponentEndpointOptions endpointOptions;
    private readonly HashSet<Assembly> includedAssemblies;
    private readonly List<Action<EndpointBuilder>> conventions;
    private readonly List<Action<EndpointBuilder>> finallyConventions;

    internal ReactiveComponentsEndpointConventionBuilder(Lock lockObject,
        ReactiveComponentEndpointOptions endpointOptions,
        HashSet<Assembly> includedAssemblies,
        List<Action<EndpointBuilder>> conventions,
        List<Action<EndpointBuilder>> finallyConventions)
    {
        this.lockObject = lockObject;
        this.endpointOptions = endpointOptions;
        this.includedAssemblies = includedAssemblies;
        this.conventions = conventions;
        this.finallyConventions = finallyConventions;
    }

    /// <summary>
    /// Include the given assemblies when searching for reactive components.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan.</param>
    public ReactiveComponentsEndpointConventionBuilder AddAdditionalAssemblies(params IEnumerable<Assembly> assemblies)
    {
        lock (lockObject)
        {
            includedAssemblies.UnionWith(assemblies);
        }

        return this;
    }

    /// <summary>
    /// Use a different scheme to generate the URLs for reactive components.
    /// </summary>
    /// <param name="routeBuilder">The function used to generate URLs.</param>
    public ReactiveComponentsEndpointConventionBuilder WithRouteBuilder(Func<Type, string> routeBuilder)
    {
        Finally(SetRoutePattern);
        return this;

        void SetRoutePattern(EndpointBuilder endpointBuilder)
        {
            if (endpointBuilder is not RouteEndpointBuilder routeEndpointBuilder)
            {
                throw new InvalidOperationException($"Expected a {nameof(RouteEndpointBuilder)} but got {endpointBuilder.GetType()}.");
            }

            var componentTypeMetadata = endpointBuilder.Metadata.OfType<ComponentTypeMetadata>().FirstOrDefault();
            if (componentTypeMetadata is null)
            {
                throw new InvalidOperationException($"Expected a {nameof(ComponentTypeMetadata)} on the endpoint, but there wasn't any.");
            }

            var route = routeBuilder(componentTypeMetadata.Type);
            routeEndpointBuilder.RoutePattern = RoutePatternFactory.Pattern(route);
        }
    }
    /// <summary>
    /// Sets a <see cref="ResourceAssetCollection" /> and <see cref="ImportMapDefinition" /> metadata
    /// for the component application.
    /// </summary>
    /// <param name="manifestPath">The manifest associated with the assets.</param>
    /// <seealso cref="RazorComponentsEndpointConventionBuilderExtensions.WithStaticAssets"/>
    public ReactiveComponentsEndpointConventionBuilder WithStaticAssets(string? manifestPath = null)
    {
        lock (lockObject)
        {
            endpointOptions.StaticAssetManifestPath = manifestPath;
        }

        return this;
    }

    /// <inheritdoc />
    public void Add(Action<EndpointBuilder> convention)
    {
        lock (lockObject)
        {
            conventions.Add(convention);
        }
    }

    /// <inheritdoc />
    public void Finally(Action<EndpointBuilder> finallyConvention)
    {
        lock (lockObject)
        {
            finallyConventions.Add(finallyConvention);
        }
    }
}
