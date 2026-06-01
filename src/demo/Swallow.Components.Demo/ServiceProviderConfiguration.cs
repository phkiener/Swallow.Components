using Microsoft.Extensions.DependencyInjection;

namespace Swallow.Components.Demo;

public static class ServiceProviderConfiguration
{
    public static IServiceCollection AddDemoServices(this IServiceCollection services)
    {
        services.AddSwallowComponents();

        return services;
    }
}
