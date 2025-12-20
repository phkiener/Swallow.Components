using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Swallow.Components.Reactive.Routing;

internal static class EndpointBuilderExtensions
{
    extension(EndpointBuilder endpointBuilder)
    {
        public void CopyAttributeMetadata(Type sourceType, Func<Attribute, bool> includeAttribute)
        {
            foreach (var attribute in sourceType.GetCustomAttributes<Attribute>(inherit: true))
            {
                if (attribute is not RequiredMemberAttribute && includeAttribute(attribute))
                {
                    endpointBuilder.Metadata.Add(attribute);
                }
            }
        }

        public void ApplyResourceCollectionMetadata(IEndpointRouteBuilder endpoints, string? manifestPath, ILogger logger)
        {
            var resolver = ResourceCollectionResolver.TryCreate(endpoints);
            if (resolver is null)
            {
                logger.LogWarning("Cannot access ResourceCollectionResolver via reflection; components rendered using static reactive rendering will not have access to the Assets collection.");
                return;
            }

            if (resolver.IsRegistered(manifestPath))
            {
                var collection = resolver.ResolveResourceCollection(manifestPath);
                var importMap = ImportMapDefinition.FromResourceCollection(collection);

                endpointBuilder.Metadata.Add(collection);
                endpointBuilder.Metadata.Add(importMap);
            }
        }
    }

    private sealed class ResourceCollectionResolver
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
}
