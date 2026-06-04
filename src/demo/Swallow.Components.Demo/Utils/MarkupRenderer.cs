using AngleSharp;
using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace Swallow.Components.Demo.Utils;

public sealed class MarkupRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
{
    private static readonly PrettyMarkupFormatter formatter = new() { NewLine = "\n", Indentation = "  " };
    private readonly HtmlRenderer htmlRenderer = new(serviceProvider, loggerFactory);

    public async Task<string> RenderAsync(RenderFragment renderFragment)
    {
        var fragmentMarkup = await RenderFragmentToMarkupAsync(renderFragment);

        return FormatMarkup(fragmentMarkup);
    }

    private Task<string> RenderFragmentToMarkupAsync(RenderFragment renderFragment)
    {
        var parameters = new Dictionary<string, object?> { [nameof(Fragment.ChildContent)] = renderFragment };
        return htmlRenderer.Dispatcher.InvokeAsync(
            async () =>
            {
                var parameterView = ParameterView.FromDictionary(parameters);
                var rootComponent = await htmlRenderer.RenderComponentAsync<Fragment>(parameterView);

                return rootComponent.ToHtmlString();
            });
    }

    private static string FormatMarkup(string markup)
    {
        var parser = new HtmlParser();
        var document = parser.ParseDocument(markup);

        var formattedNodes = document.Body?.Children.Select(c => c.ToHtml(formatter)) ?? [];
        return string.Join("", formattedNodes);
    }

    private sealed class Fragment : ComponentBase
    {
        [Parameter]
        [EditorRequired]
        public required RenderFragment ChildContent { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, ChildContent);
        }
    }
}
