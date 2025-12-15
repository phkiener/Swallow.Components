using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Swallow.Components.Reactive.Framework;

namespace Swallow.Components.Reactive.Routing;

internal sealed class ReactiveComponentEndpointOptions
{
    public string? StaticAssetManifestPath { get; set; }
}

public sealed class ReactiveComponentsEndpointDataSource : EndpointDataSource
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<ReactiveComponentsEndpointDataSource> logger;
    private readonly IEndpointRouteBuilder endpointRouteBuilder;
    private readonly ReactiveComponentEndpointOptions endpointOptions = new();
    private readonly Lock lockObject = new();
    private readonly HashSet<Assembly> includedAssemblies = [];
    private readonly List<Action<EndpointBuilder>> conventions = [];
    private readonly List<Action<EndpointBuilder>> finallyConventions = [];

    private List<Endpoint>? endpoints;
    private CancellationTokenSource changeTokenSource;
    private IChangeToken changeToken;

    public ReactiveComponentsEndpointConventionBuilder ConventionBuilder { get; }

    public ReactiveComponentsEndpointDataSource(
        IServiceProvider serviceProvider,
        ILogger<ReactiveComponentsEndpointDataSource> logger,
        IEndpointRouteBuilder endpointRouteBuilder)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        this.endpointRouteBuilder = endpointRouteBuilder;

        ConventionBuilder = new ReactiveComponentsEndpointConventionBuilder(
            lockObject: lockObject,
            endpointOptions: endpointOptions,
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

    private RouteEndpointBuilder BuildEndpoint(Type targetType)
    {
        var routeTemplate = $"_framework/reactive/{targetType.Assembly.GetName().Name}/{targetType.FullName}";

        var endpointBuilder = new RouteEndpointBuilder(
            requestDelegate: null,
            routePattern: RoutePatternFactory.Parse(routeTemplate),
            order: 0);

        var result = RequestDelegateFactory.Create(
            RenderReactiveComponent,
            new RequestDelegateFactoryOptions
            {
                EndpointBuilder = endpointBuilder,
                ServiceProvider = serviceProvider,
                DisableInferBodyFromParameters = true
            });

        endpointBuilder.RequestDelegate = result.RequestDelegate;
        endpointBuilder.DisplayName = $"{endpointBuilder.RoutePattern.RawText} ({targetType.Name})";

        endpointBuilder.CopyAttributeMetadata(targetType, static a => a is not ReactiveComponentAttribute and not RouteAttribute);
        endpointBuilder.ApplyResourceCollectionMetadata(endpointRouteBuilder, endpointOptions.StaticAssetManifestPath, logger);

        endpointBuilder.Metadata.Add(new SuppressLinkGenerationMetadata());
        endpointBuilder.Metadata.Add(new RequireAntiforgeryTokenAttribute());
        endpointBuilder.Metadata.Add(new HttpMethodMetadata([HttpMethods.Post]));
        endpointBuilder.Metadata.Add(new ComponentTypeMetadata(targetType));

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
            var notFoundResult = TypedResults.NotFound();
            return notFoundResult.ExecuteAsync(httpContext);
        }

        var invoker = httpContext.RequestServices.GetRequiredService<ReactiveComponentInvoker>();
        return invoker.InvokeAsync(componentType, httpContext);
    }

}
