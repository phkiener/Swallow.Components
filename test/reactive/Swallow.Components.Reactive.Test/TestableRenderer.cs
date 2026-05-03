using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;

namespace Swallow.Components.Reactive.Test;

public sealed class TestableRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : StaticHtmlRenderer(serviceProvider, loggerFactory)
{
    public Task Render(RenderFragment renderFragment)
    {
        var hostParameters = new Dictionary<string, object?> { [nameof(ContentHost.Body)] = renderFragment };

        return Dispatcher.InvokeAsync(async () =>
        {
            var root = BeginRenderingComponent(typeof(ContentHost), ParameterView.FromDictionary(hostParameters));
            await root.QuiescenceTask;
        });
    }

    public ArrayRange<RenderTreeFrame> GetFrames(int componentId) => GetCurrentRenderTreeFrames(componentId);

    private sealed class ContentHost : ComponentBase
    {
        [Parameter]
        [EditorRequired]
        public required RenderFragment Body { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, Body);
        }
    }
}
