using Microsoft.Extensions.DependencyInjection;
using Swallow.Components.Demo.Utils;

namespace Swallow.Components.Demo;

public static class ServiceProviderConfiguration
{
    public static IServiceCollection AddDemoServices(this IServiceCollection services)
    {
        services.AddSwallowComponents();
        services.AddTransient<MarkupRenderer>();
        services.AddTransient<RazorRenderer>();

        return services;
    }
}
