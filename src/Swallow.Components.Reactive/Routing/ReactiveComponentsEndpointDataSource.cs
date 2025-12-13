using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Swallow.Components.Reactive.Framework;

namespace Swallow.Components.Reactive.Routing;

public sealed class ReactiveComponentsEndpointDataSource : EndpointDataSource
{
    private readonly Lock lockObject = new();
    private readonly HashSet<Assembly> includedAssemblies = [];
    private readonly List<Action<EndpointBuilder>> conventions = [];
    private readonly List<Action<EndpointBuilder>> finallyConventions = [];

    private List<Endpoint>? endpoints;
    private CancellationTokenSource changeTokenSource;
    private IChangeToken changeToken;

    public ReactiveComponentsEndpointConventionBuilder ConventionBuilder { get; }

    public ReactiveComponentsEndpointDataSource()
    {
        ConventionBuilder = new ReactiveComponentsEndpointConventionBuilder(
            lockObject: lockObject,
            includedAssemblies: includedAssemblies,
            conventions: conventions,
            finallyConventions: finallyConventions);

        GenerateChangeToken();
    }

    public override IChangeToken GetChangeToken() => changeToken;

    public override IReadOnlyList<Endpoint> Endpoints => GetEndpoints();

    private List<Endpoint> GetEndpoints()
    {
        if (endpoints is null)
        {
            BuildEndpoints();
            GenerateChangeToken();
        }

        return endpoints;
    }

    [MemberNotNull(nameof(endpoints))]
    private void BuildEndpoints()
    {
        var foundEndpoints = new List<Endpoint>();
        lock (lockObject)
        {
            var types = includedAssemblies.SelectMany(static a => a.GetExportedTypes())
                .Where(static a => a.GetCustomAttributes<ReactiveComponentAttribute>().Any())
                .ToList();

            foreach (var type in types)
            {
                var builder = BuildEndpoint(type);
                foreach (var convention in conventions)
                {
                    convention(builder);
                }

                foreach (var finalConvention in finallyConventions)
                {
                    finalConvention(builder);
                }

                foundEndpoints.Add(builder.Build());
            }
        }

        endpoints = foundEndpoints;
    }

    private static RouteEndpointBuilder BuildEndpoint(Type targetType)
    {
        var routeTemplate = $"_framework/reactive/{targetType.Assembly.GetName().Name}/{targetType.FullName}";

        var endpointBuilder = new RouteEndpointBuilder(
            requestDelegate: RenderReactiveComponent,
            routePattern: RoutePatternFactory.Parse(routeTemplate),
            order: 0);

        endpointBuilder.DisplayName = $"{endpointBuilder.RoutePattern.RawText} ({targetType.Name})";
        endpointBuilder.Metadata.Add(routeTemplate);
        endpointBuilder.Metadata.Add(new HttpMethodMetadata([HttpMethods.Get, HttpMethods.Post]));
        endpointBuilder.Metadata.Add(new ComponentTypeMetadata(targetType));

        endpointBuilder.AddEmptyRenderMode();
        endpointBuilder.CopyAttributeMetadata(targetType, static a => a is not ReactiveComponentAttribute);

        return endpointBuilder;
    }

    [MemberNotNull(nameof(changeTokenSource))]
    [MemberNotNull(nameof(changeToken))]
    private void GenerateChangeToken()
    {
        var previousChangeTokenSource = changeTokenSource;
        changeTokenSource = new CancellationTokenSource();
        changeToken = new CancellationChangeToken(changeTokenSource.Token);

        previousChangeTokenSource?.Cancel();
        previousChangeTokenSource?.Dispose();
    }

    private static Task RenderReactiveComponent(HttpContext httpContext)
    {
        var componentType = httpContext.GetEndpoint()?.Metadata.GetMetadata<ComponentTypeMetadata>()?.Type;
        if (componentType is null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }

        var invoker = httpContext.RequestServices.GetRequiredService<ReactiveComponentInvoker>();
        return invoker.InvokeAsync(componentType, httpContext);
    }

}
