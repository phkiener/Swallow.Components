using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Swallow.Components.Reactive.Framework;

public sealed class ReactiveRouteView : RouteView
{
    private static readonly ConcurrentDictionary<Type, bool> isReactivePageCache = new();
    private static readonly ConcurrentDictionary<Type, Type?> layoutAttributeCache = new();

    protected override void Render(RenderTreeBuilder builder)
    {
        var isReactive = isReactivePageCache.GetOrAdd(RouteData.PageType, ResolvePageTypeInfo);
        if (!isReactive)
        {
            base.Render(builder);
            return;
        }

        var pageLayout = layoutAttributeCache.GetOrAdd(RouteData.PageType, ResolvePageLayout) ?? DefaultLayout;
        builder.OpenComponent<LayoutView>(100); // start at 100 to offset the base.Render, even though it *should* never matter
        builder.AddComponentParameter(101, nameof(LayoutView.Layout), pageLayout);
        builder.AddComponentParameter(102, nameof(LayoutView.ChildContent), (RenderTreeBuilder content) =>
        {
            content.OpenComponent(1, typeof(ReactiveComponentBoundary));
            content.AddComponentParameter(2, nameof(ReactiveComponentBoundary.ComponentType), RouteData.PageType);
            content.CloseComponent();
        });
        builder.CloseComponent();
    }

    private static Type? ResolvePageLayout(Type pageType)
    {
        return pageType.GetCustomAttributes<LayoutAttribute>().FirstOrDefault()?.LayoutType;
    }

    private static bool ResolvePageTypeInfo(Type pageType)
    {
        return pageType.GetCustomAttributes<ReactiveComponentAttribute>().FirstOrDefault() is not null;
    }
}
