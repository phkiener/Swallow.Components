using System.Diagnostics;

namespace Swallow.Components.Reactive.Rendering.EventHandlers;

internal readonly record struct ComponentEventDescriptor(ulong EventHandlerId, int ComponentId, string EventName, string ElementPath);

[DebuggerDisplay("<{tagName,nq}> (Component {componentId,nq})")]
internal sealed class EventTreeNode(string tag, int componentId)
{
    private readonly record struct ComponentEventHandler(ulong EventHandlerId, string EventName);

    private string? id;
    private readonly string tagName = tag;
    private readonly int componentId = componentId;
    private readonly List<EventTreeNode> children = [];
    private readonly List<ComponentEventHandler> handlers = [];

    public EventTreeNode AddChild(string tag, int containingComponentId)
    {
        var node = new EventTreeNode(tag, containingComponentId);
        children.Add(node);

        return node;
    }

    public void AddHandler(ulong eventHandlerId, string eventName)
    {
        var handler = new ComponentEventHandler(eventHandlerId, eventName);
        handlers.Add(handler);
    }

    public void SetId(object idAttribute)
    {
        id = idAttribute.ToString();
    }

    public IEnumerable<ComponentEventDescriptor> EnumerateDescriptors()
    {
        return EnumerateDescriptors(tagName);
    }

    private IEnumerable<ComponentEventDescriptor> EnumerateDescriptors(string prefix)
    {
        if (id is not null)
        {
            prefix = $"#{id}";
        }

        var directDescriptors = handlers.Select(h => new ComponentEventDescriptor(h.EventHandlerId, componentId, h.EventName, prefix));
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

    public static EventTreeNode CreateRootNode(int rootComponentId)
    {
        return new EventTreeNode(string.Empty, rootComponentId);
    }
}
