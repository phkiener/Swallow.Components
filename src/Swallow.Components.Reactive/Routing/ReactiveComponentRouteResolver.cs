using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace Swallow.Components.Reactive.Routing;

internal sealed class ReactiveComponentRouteResolver(IEnumerable<EndpointDataSource> dataSources)
{
    private static readonly ConcurrentDictionary<Type, string?> resolvedRoutes = new();

    public string? ResolveRoute(Type componentType)
    {
        return resolvedRoutes.GetOrAdd(componentType, ResolveRouteInternal);
    }

    private string? ResolveRouteInternal(Type type)
    {
        var possibleDataSources = FindRelevantDataSources<ReactiveComponentsEndpointDataSource>(dataSources);
        var endpoint = possibleDataSources
            .SelectMany(ds => ds.Endpoints)
            .OfType<RouteEndpoint>()
            .FirstOrDefault(ep => ep.Metadata.GetRequiredMetadata<ComponentTypeMetadata>().Type == type);

        return endpoint?.RoutePattern.RawText;
    }

    private static IEnumerable<T> FindRelevantDataSources<T>(IEnumerable<EndpointDataSource> dataSources) where T : EndpointDataSource
    {
        var dataSourceQueue = new Queue<EndpointDataSource>(dataSources);
        while (dataSourceQueue.TryDequeue(out var dataSource))
        {
            if (dataSource is T matchingDataSource)
            {
                yield return matchingDataSource;
            }

            if (dataSource is CompositeEndpointDataSource compositeDataSource)
            {
                foreach (var innerDataSource in compositeDataSource.DataSources)
                {
                    dataSourceQueue.Enqueue(innerDataSource);
                }
            }
        }
    }
}
