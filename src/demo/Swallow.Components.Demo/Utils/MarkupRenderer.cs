using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

namespace Swallow.Components.Demo.Utils;

public sealed class MarkupRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
{
    private static readonly XmlWriterSettings XmlWriterSettings = new() { OmitXmlDeclaration = true, Indent = true };
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
        var builder = new StringBuilder();
        using var xmlWriter = XmlTextWriter.Create(builder, XmlWriterSettings);

        var document = XDocument.Parse(markup);
        foreach (var node in document.DescendantNodes().OfType<XText>())
        {
            var depth = Depth(node) - 1;

            var leadingWhitespace = node.PreviousNode is null
                ? $"\n{new string(' ', 2 * (depth + 1))}"
                : $"\n\n{new string(' ', 2 * (depth + 1))}";
            var trailingWhitespace = $"\n{new string(' ', 2 * depth)}";

            node.Value = $"{leadingWhitespace}{node.Value.Trim()}{trailingWhitespace}";
        }

        document.WriteTo(xmlWriter);
        xmlWriter.Flush();

        return builder.ToString();
    }

    private static int Depth(XNode node)
    {
        var depth = 0;
        XNode? current = node;
        while (current?.Parent != null)
        {
            current = current.Parent;
            depth++;
        }

        return depth;
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
