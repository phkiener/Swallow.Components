using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Routing;
using Swallow.Components.Reactive.Shims;

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

        public void ApplyResourceCollectionMetadata(IEndpointRouteBuilder endpoints, string? manifestPath)
        {
            var resolver = ResourceCollectionResolver.TryCreate(endpoints);
            if (resolver is null)
            {
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
}
