using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Swallow.Components.Demo.Utils;

[SuppressMessage("Usage", "BL0006")]
public sealed class RazorRenderer
{
    private static readonly XmlWriterSettings XmlWriterSettings = new() { OmitXmlDeclaration = true, Indent = true };

    public Task<string> RenderAsync(RenderFragment renderFragment)
    {
        var elements = Render(renderFragment).Cast<object>().ToArray();
        var document = new XDocument();
        document.Add(elements);

        var builder = new StringBuilder();
        using var xmlWriter = XmlTextWriter.Create(builder, XmlWriterSettings);

        document.WriteTo(xmlWriter);
        xmlWriter.Flush();

        return Task.FromResult(builder.ToString());
    }

    private static IEnumerable<XNode> Render(RenderFragment fragment)
    {
        var renderTreeBuilder = new RenderTreeBuilder();
        fragment.Invoke(renderTreeBuilder);

        var frames = renderTreeBuilder.GetFrames();
        for (var i = 0; i < frames.Count;)
        {
            ref var frame = ref frames.Array[i];

            // TODO: Handle all the frame types.
            if (frame.FrameType is RenderTreeFrameType.Component)
            {
                var element = new XElement(frame.ComponentType.Name);
                i += 1;

                for (var j = i; j < i + frame.ComponentSubtreeLength; ++j)
                {
                    ref var innerFrame = ref frames.Array[j];
                    if (innerFrame.FrameType is RenderTreeFrameType.Attribute)
                    {
                        if (innerFrame.AttributeValue is RenderFragment innerFragment)
                        {
                            var elements = Render(innerFragment).Cast<object>().ToArray();
                            if (innerFrame.AttributeName is "ChildContent")
                            {
                                element.Add(elements);
                            }
                            else
                            {
                                var parameterElement = new XElement(innerFrame.AttributeName);
                                parameterElement.Add(elements);

                                element.Add(parameterElement);
                            }
                        }
                    }
                }

                i += frame.ComponentSubtreeLength;

                yield return element;
                continue;
            }

            if (frame.FrameType is RenderTreeFrameType.Markup)
            {
                var element = new XText(frame.MarkupContent);
                i += 1;

                yield return element;
                continue;
            }

            if (frame.FrameType is RenderTreeFrameType.Text)
            {
                var element = new XText(frame.TextContent);
                i += 1;

                yield return element;
                continue;
            }

        }
    }
}
