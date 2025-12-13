using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Swallow.Components.Reactive.EventHandlers;
using Swallow.Components.Reactive.Framework;
using Swallow.Components.Reactive.Routing;

namespace Swallow.Components.Reactive;

public static class ServiceProviderConfig
{
    public static void AddReactiveComponents(this IServiceCollection services)
    {
        services.AddScoped<HandlerRegistration>();
        services.AddScoped<ReactiveComponentInvoker>();
        services.AddScoped<ReactiveComponentRenderer>();
    }

    public static IRazorComponentsBuilder AddReactiveComponents(this IRazorComponentsBuilder razorComponentsBuilder)
    {
        razorComponentsBuilder.Services.AddScoped<HandlerRegistration>();
        razorComponentsBuilder.Services.AddScoped<ReactiveComponentInvoker>();
        razorComponentsBuilder.Services.AddScoped<ReactiveComponentRenderer>();

        return razorComponentsBuilder;
    }

    public static ReactiveComponentsEndpointConventionBuilder MapReactiveComponents(this IEndpointRouteBuilder endpoints)
    {
        var dataSource = endpoints.DataSources.OfType<ReactiveComponentsEndpointDataSource>().FirstOrDefault();
        if (dataSource is null)
        {
            dataSource = ActivatorUtilities.CreateInstance<ReactiveComponentsEndpointDataSource>(endpoints.ServiceProvider);
            endpoints.DataSources.Add(dataSource);
        }

        var initialAssemblies = new[] { Assembly.GetEntryAssembly(), Assembly.GetCallingAssembly() };
        return dataSource.ConventionBuilder.AddAdditionalAssemblies(initialAssemblies.OfType<Assembly>());
    }
}
