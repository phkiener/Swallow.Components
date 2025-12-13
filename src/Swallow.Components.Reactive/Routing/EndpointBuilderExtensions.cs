using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Endpoints;

namespace Swallow.Components.Reactive.Routing;

internal static class EndpointBuilderExtensions
{
    private static object? NoRenderModeMetadata { get; } = BuildRenderModeMetadata();

    private static object? BuildRenderModeMetadata()
    {
        var definingAssembly = typeof(IRazorComponentEndpointInvoker).Assembly;
        var metadataType = definingAssembly.GetType("Microsoft.AspNetCore.Components.Endpoints.ConfiguredRenderModesMetadata");

        // Yes, we could set StaticReactive as render mode here.
        // But the render will actually complain
        return metadataType is null ? null : Activator.CreateInstance(metadataType, [Array.Empty<IComponentRenderMode>()]);
    }

    extension(EndpointBuilder endpointBuilder)
    {
        public void AddEmptyRenderMode()
        {
            // If the metadata is null, something has changed in the framework and we haven't noticed. That's okay.
            // The endpoint renderer will write (empty) state into a comment in the DOM... useless and noisy but not an issue.
            if (NoRenderModeMetadata is not null)
            {
                endpointBuilder.Metadata.Add(NoRenderModeMetadata);
            }
        }

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
    }
}
