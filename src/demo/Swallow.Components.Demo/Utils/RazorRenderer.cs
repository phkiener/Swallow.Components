using System.Diagnostics;
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

    private static object?[] Render(RenderFragment fragment)
    {
        var renderTreeBuilder = new RenderTreeBuilder();
        fragment.Invoke(renderTreeBuilder);

        var frames = renderTreeBuilder.GetFrames();
        var nodes = Render(frames);

        return nodes;
    }

    private static object?[] Render(ArrayRange<RenderTreeFrame> frames)
    {
        var nodes = new List<XNode>();
        for (var index = 0; index < frames.Count;)
        {
            ref var currentFrame = ref frames.Array[index];
            switch (currentFrame.FrameType)
            {
                case RenderTreeFrameType.Element:
                    var element = RenderElement(frames, ref index);
                    nodes.Add(element);

                    break;

                case RenderTreeFrameType.Component:
                    var component = RenderComponent(frames, ref index);
                    nodes.Add(component);

                    break;

                case RenderTreeFrameType.Text:
                    nodes.Add(new XText(currentFrame.TextContent));
                    index += 1;

                    break;
                case RenderTreeFrameType.Markup:
                    var parsedMarkup = XDocument.Parse($"<root>{currentFrame.MarkupContent}</root>");
                    foreach (var node in parsedMarkup.DescendantNodes().OfType<XText>())
                    {
                        var depth = Depth(node);

                        var leadingWhitespace = node.PreviousNode is null
                            ? $"\n{new string(' ', 2 * (depth + 1))}"
                            : $"\n\n{new string(' ', 2 * (depth + 1))}";
                        var trailingWhitespace = $"\n{new string(' ', 2 * depth)}";

                        node.Value = $"{leadingWhitespace}{node.Value.Trim()}{trailingWhitespace}";
                    }

                    nodes.AddRange(parsedMarkup.Root?.Nodes() ?? []);
                    index += 1;

                    break;
                default:
                    index += 1;
                    break;
            }
        }

        return nodes.Cast<object?>().ToArray();
    }

    private static XElement RenderComponent(ArrayRange<RenderTreeFrame> frames, ref int index)
    {
        ref var componentFrame = ref frames.Array[index];
        Debug.Assert(componentFrame.FrameType is RenderTreeFrameType.Component, "Starting frame is not of type Component");

        index += 1;
        var component = new XElement(componentFrame.ComponentType.Name);
        for (var offset = 1; offset < componentFrame.ComponentSubtreeLength; ++offset, ++index)
        {
            ref var innerFrame = ref frames.Array[index];
            if (innerFrame.FrameType is RenderTreeFrameType.Attribute)
            {
                if (innerFrame.AttributeValue is RenderFragment innerFragment)
                {
                    var element = Render(innerFragment);
                    if (innerFrame.AttributeName is "ChildContent")
                    {
                        component.Add(element);
                    }
                    else
                    {
                        component.Add(new XElement(innerFrame.AttributeName, element));
                    }

                }
                else
                {
                    component.SetAttributeValue(innerFrame.AttributeName, innerFrame.AttributeValue);
                }
            }
        }

        return component;
    }

    private static XElement RenderElement(ArrayRange<RenderTreeFrame> frames, ref int index)
    {
        ref var elementFrame = ref frames.Array[index];
        Debug.Assert(elementFrame.FrameType is RenderTreeFrameType.Element, "Starting frame is not of type Element");

        index += 1;
        var element = new XElement(elementFrame.ElementName);
        for (var offset = 0; offset < elementFrame.ElementSubtreeLength; ++offset, ++index)
        {
            ref var innerFrame = ref frames.Array[index];
            if (innerFrame.FrameType is RenderTreeFrameType.Attribute)
            {
                element.SetAttributeValue(innerFrame.AttributeName, innerFrame.AttributeValue);
            }
        }

        return element;
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
}
