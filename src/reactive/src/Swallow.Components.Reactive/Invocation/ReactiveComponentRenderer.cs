using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;

namespace Swallow.Components.Reactive.Invocation;

internal sealed partial class ReactiveComponentRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : StaticHtmlRenderer(serviceProvider, loggerFactory)
{
    private Type? reactiveComponentType;
    private int? reactiveFragmentComponentId;

    protected override ComponentState CreateComponentState(int componentId, IComponent component, ComponentState? parentComponentState)
    {
        if (reactiveComponentType is null && component.GetType() == reactiveComponentType)
        {
            reactiveFragmentComponentId = componentId;
        }

        return base.CreateComponentState(componentId, component, parentComponentState);
    }

    public Task RehydrateComponent(Type componentType)
    {
        reactiveComponentType = componentType;

        var layoutParameters = new Dictionary<string, object?>
        {
            [nameof(LayoutView.Layout)] = typeof(ReactiveFragmentContainer),
            [nameof(LayoutView.ChildContent)] = (RenderTreeBuilder builder) =>
            {
                builder.OpenComponent(0, reactiveComponentType);
                builder.CloseComponent();
            }
        };

        return Dispatcher.InvokeAsync(() =>
        {
            var rootComponent = BeginRenderingComponent(typeof(LayoutView), ParameterView.FromDictionary(layoutParameters));
            return rootComponent.QuiescenceTask;
        });
    }

    public Task WaitForSettled()
    {
        return dispatcher.ProcessAsync();
    }

    public Task<string> RenderMarkupAsync()
    {
        return Dispatcher.InvokeAsync(() =>
        {
            var stringWriter = new StringWriter();
            WriteComponentHtml(0, stringWriter);

            return stringWriter.ToString();
        });
    }
}
