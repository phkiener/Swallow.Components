using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Swallow.Components.Reactive.Framework;

public sealed class ReactiveRouteView : RouteView
{
    private static readonly ConcurrentDictionary<Type, bool> isReactivePageCache = new();

    protected override void Render(RenderTreeBuilder builder)
    {
        if (!isReactivePageCache.GetOrAdd(RouteData.PageType, IsReactivePage))
        {
            base.Render(builder);
            return;
        }

        builder.OpenComponent(100, typeof(ReactiveComponentBoundary));
        builder.AddComponentParameter(101, nameof(ReactiveComponentBoundary.ComponentType), RouteData.PageType);
        builder.CloseComponent();
    }

    private static bool IsReactivePage(Type pageType) => pageType.GetCustomAttributes<ReactiveComponentAttribute>().Any();
}
