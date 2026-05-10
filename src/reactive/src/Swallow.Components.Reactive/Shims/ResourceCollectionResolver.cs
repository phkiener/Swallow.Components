using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace Swallow.Components.Reactive.Shims;

internal sealed class ResourceCollectionResolver
{
    private static readonly Type? underlyingType = typeof(IRazorComponentEndpointInvoker).Assembly.GetType("Microsoft.AspNetCore.Components.Endpoints.ResourceCollectionResolver");
    private static MethodInfo? resolveMethod;
    private static MethodInfo? isRegisteredMethod;

    private readonly object instance;
    private readonly Func<object, string?, bool> isRegistered;
    private readonly Func<object, string?, ResourceAssetCollection> resolve;

    private ResourceCollectionResolver(object instance, Func<object, string?, bool> isRegistered, Func<object, string?, ResourceAssetCollection> resolve)
    {
        this.instance = instance;
        this.isRegistered = isRegistered;
        this.resolve = resolve;
    }

    public bool IsRegistered(string? manifestName)
    {
        return isRegistered(instance, manifestName);
    }

    public ResourceAssetCollection ResolveResourceCollection(string? manifestName)
    {
        return resolve(instance, manifestName);
    }

    public static ResourceCollectionResolver? TryCreate(IEndpointRouteBuilder endpoints)
    {
        if (underlyingType is null)
        {
            return null;
        }

        resolveMethod ??= underlyingType.GetMethod("ResolveResourceCollection");
        isRegisteredMethod ??= underlyingType.GetMethod("IsRegistered");

        if (resolveMethod is null || isRegisteredMethod is null)
        {
            return null;
        }

        var instance = Activator.CreateInstance(underlyingType, endpoints);
        if (instance is null)
        {
            return null;
        }

        return new ResourceCollectionResolver(
            instance: instance,
            isRegistered: static (instance, manifest) => (bool?)isRegisteredMethod.Invoke(instance, [manifest]) ?? false,
            resolve: static (instance, manifest) => (ResourceAssetCollection?)resolveMethod.Invoke(instance, [manifest]) ?? ResourceAssetCollection.Empty);
    }
}
