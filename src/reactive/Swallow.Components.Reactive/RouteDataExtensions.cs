using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Swallow.Components.Reactive.Framework;

/// <summary>
/// Extensions for <see cref="RouteData"/>.
/// </summary>
public static class RouteDataExtensions
{
    private static readonly ConcurrentDictionary<Type, bool> isReactivePageCache = new();

    /// <summary>
    /// Overwrite the given <see cref="RouteData"/> with an instance that wraps a reactive host
    /// around the page, given that the routed component does have the <see cref="ReactiveComponentAttribute"/>.
    /// </summary>
    /// <param name="routeData">The route data to use.</param>
    /// <returns>The adjusted (if needed) <see cref="RouteData"/>.</returns>
    public static RouteData AsReactiveIfNeeded(this RouteData routeData)
    {
        var isReactivePage = isReactivePageCache.GetOrAdd(routeData.PageType, ResolveIsReactive);
        if (isReactivePage)
        {
            var reactivePageHost = typeof(ReactivePageHost<>).MakeGenericType(routeData.PageType);
            return new RouteData(reactivePageHost, routeData.RouteValues);
        }

        return routeData;
    }

    private static bool ResolveIsReactive(Type pageType)
    {
        return pageType.GetCustomAttributes<ReactiveComponentAttribute>().FirstOrDefault() is not null;
    }

    private sealed class ReactivePageHost<T> : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<ReactiveComponentBoundary>(0);
            builder.AddComponentParameter(1, nameof(ReactiveComponentBoundary.ComponentType), typeof(T));
            builder.CloseComponent();
        }
    }
}
