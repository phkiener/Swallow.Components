using System.Diagnostics;

namespace Swallow.Components.Reactive.EventHandlers;

internal readonly record struct ComponentEventDescriptor(ulong EventHandlerId, int ComponentId, string EventName, string ElementPath);

[DebuggerDisplay("<{tagName,nq}>")]
internal sealed class EventTreeNode(string tag)
{
    private readonly record struct ComponentEventHandler(ulong EventHandlerId, int ComponentId, string EventName);

    private readonly string tagName = tag;
    private readonly List<EventTreeNode> children = [];
    private readonly List<ComponentEventHandler> handlers = [];

    public EventTreeNode AddChild(string tag)
    {
        var node = new EventTreeNode(tag);
        children.Add(node);

        return node;
    }

    public void AddHandler(ulong eventHandlerId, int componentId, string eventName)
    {
        var handler = new ComponentEventHandler(eventHandlerId, componentId, eventName);
        handlers.Add(handler);
    }

    public IEnumerable<ComponentEventDescriptor> EnumerateDescriptors()
    {
        return EnumerateDescriptors(tagName);
    }

    private IEnumerable<ComponentEventDescriptor> EnumerateDescriptors(string prefix)
    {
        var directDescriptors = handlers.Select(h => new ComponentEventDescriptor(h.EventHandlerId, h.ComponentId, h.EventName, prefix));
        var descendantDescriptors = new List<IEnumerable<ComponentEventDescriptor>>();

        foreach (var tagGroup in children.GroupBy(static n => n.tagName))
        {
            var entries = tagGroup.ToList();

            var childDescriptors = entries is [var singleEntry]
                ? singleEntry.EnumerateDescriptors($"{prefix}/{tagGroup.Key}")
                : entries.SelectMany((e, i) => e.EnumerateDescriptors($"{prefix}/{tagGroup.Key}[{i}]"));

            descendantDescriptors.Add(childDescriptors);
        }

        return directDescriptors.Concat(descendantDescriptors.SelectMany(static descriptors => descriptors));
    }

    public static EventTreeNode CreateRootNode()
    {
        return new EventTreeNode(string.Empty);
    }
}
