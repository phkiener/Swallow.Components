using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Swallow.Components.Reactive.Routing;

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

    public ReactiveComponentsEndpointConventionBuilder AddAdditionalAssemblies(params IEnumerable<Assembly> assemblies)
    {
        lock (lockObject)
        {
            includedAssemblies.UnionWith(assemblies);
        }

        return this;
    }

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

    public ReactiveComponentsEndpointConventionBuilder WithStaticAssets(string? manifestPath = null)
    {
        lock (lockObject)
        {
            endpointOptions.StaticAssetManifestPath = manifestPath;
        }

        return this;
    }

    public void Add(Action<EndpointBuilder> convention)
    {
        lock (lockObject)
        {
            conventions.Add(convention);
        }
    }

    public void Finally(Action<EndpointBuilder> finallyConvention)
    {
        lock (lockObject)
        {
            finallyConventions.Add(finallyConvention);
        }
    }
}
