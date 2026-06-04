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
        document.Add(new XElement("root", elements));

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

        var nodes = new List<object?>();
        var index = 0;
        while (index < frames.Count)
        {

            var currentFrameNodes = RenderFrame(frames, ref index);
            nodes.AddRange(currentFrameNodes);
        }

        return nodes.ToArray();
    }

    private static object?[] RenderFrame(ArrayRange<RenderTreeFrame> frames, ref int index)
    {
        ref var currentFrame = ref frames.Array[index];
        switch (currentFrame.FrameType)
        {
            case RenderTreeFrameType.Text:
                index += 1;
                return [new XText(currentFrame.TextContent)];

            case RenderTreeFrameType.Markup:
                index += 1;

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

                return parsedMarkup.Root is null ? [] : [..parsedMarkup.Root.Nodes()];

            case RenderTreeFrameType.Attribute:
                index += 1;

                if (currentFrame.AttributeValue is RenderFragment renderFragment)
                {
                    var renderedFragment = Render(renderFragment);

                    return currentFrame.AttributeName is "ChildContent"
                        ? renderedFragment
                        : [new XElement(currentFrame.AttributeName, renderedFragment)];
                }

                return [new XAttribute(currentFrame.AttributeName, currentFrame.AttributeValue)];

            case RenderTreeFrameType.Element:
                var element = RenderElement(frames, ref index);
                return [element];

            case RenderTreeFrameType.Component:
                var component = RenderComponent(frames, ref index);
                return [component];

            default:
                index += 1;
                return [];
        }

    }

    private static XElement RenderComponent(ArrayRange<RenderTreeFrame> frames, ref int index)
    {
        ref var componentFrame = ref frames.Array[index];
        Debug.Assert(componentFrame.FrameType is RenderTreeFrameType.Component, "Starting frame is not of type Component");

        var component = new XElement(componentFrame.ComponentType.Name);
        var endFrame = index + componentFrame.ElementSubtreeLength;

        index += 1;
        for (; index < endFrame;)
        {
            var rendered = RenderFrame(frames, ref index);
            component.Add(rendered);
        }

        return component;
    }

    private static XElement RenderElement(ArrayRange<RenderTreeFrame> frames, ref int index)
    {
        ref var elementFrame = ref frames.Array[index];
        Debug.Assert(elementFrame.FrameType is RenderTreeFrameType.Element, "Starting frame is not of type Element");

        var element = new XElement(elementFrame.ElementName);
        var endFrame = index + elementFrame.ElementSubtreeLength;

        index += 1;
        for (; index < endFrame;)
        {
            var rendered = RenderFrame(frames, ref index);
            element.Add(rendered);
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
