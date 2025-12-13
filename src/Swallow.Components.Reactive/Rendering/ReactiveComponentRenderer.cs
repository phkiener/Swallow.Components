using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Swallow.Components.Reactive.EventHandlers;

namespace Swallow.Components.Reactive.Framework;

internal class ReactiveComponentRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : StaticHtmlRenderer(serviceProvider, loggerFactory)
{
    private int? rootComponentId;
    private int? fragmentComponentId;

    protected override RendererInfo RendererInfo => new("static-interactive", true);

    protected override IComponentRenderMode GetComponentRenderMode(IComponent component)
    {
        return RenderMode.StaticReactive;
    }

    protected override ComponentState CreateComponentState(int componentId, IComponent component, ComponentState? parentComponentState)
    {
        if (parentComponentState is null)
        {
            rootComponentId = componentId;
        }

        if (parentComponentState?.ComponentId == rootComponentId && component is DynamicComponent)
        {
            fragmentComponentId = componentId;
        }

        return base.CreateComponentState(componentId, component, parentComponentState);
    }

    public Task RenderFragmentAsync(Type renderedComponent)
    {
        var fragmentParameters = new Dictionary<string, object?>
        {
            [nameof(ReactiveFragment.ComponentType)] = renderedComponent
        };

        var root = BeginRenderingComponent(typeof(ReactiveFragment), ParameterView.FromDictionary(fragmentParameters));
        return root.QuiescenceTask;
    }

    public void DiscoverEventHandlers(HandlerRegistration registration)
    {
        if (fragmentComponentId is null)
        {
            return;
        }

        registration.DiscoverEventDescriptors(fragmentComponentId.Value, GetCurrentRenderTreeFrames);
    }

    public void RenderHtml(TextWriter output)
    {
        if (rootComponentId is null)
        {
            return;
        }

        WriteComponentHtml(rootComponentId.Value, output);
    }
}
