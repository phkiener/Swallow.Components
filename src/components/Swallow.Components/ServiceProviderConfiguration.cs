using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Swallow.Components.Utils;

namespace Swallow.Components;

/// <summary>
/// Extensions to register the Swallow components in a <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceProviderConfiguration
{
    /// <summary>
    /// Add the required services to the given <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to which to add the services.</param>
    public static IServiceCollection AddSwallowComponents(this IServiceCollection services)
    {
        services.TryAddScoped<IElementIdFactory, DefaultElementIdFactory>();
        services.TryAddTransient<ElementId>(static sp => sp.GetRequiredService<IElementIdFactory>().Create());

        return services;
    }
}
