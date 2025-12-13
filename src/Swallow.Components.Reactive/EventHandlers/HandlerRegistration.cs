using Microsoft.AspNetCore.Components.RenderTree;

namespace Swallow.Components.Reactive.EventHandlers;

internal delegate ArrayRange<RenderTreeFrame> ComponentFrameRetriever(int componentId);

internal sealed class HandlerRegistration
{
    private readonly List<ComponentEventDescriptor> descriptors = [];

    public event EventHandler? OnHandlersDiscovered;

    public IEnumerable<ComponentEventDescriptor> Descriptors => descriptors;

    public ComponentEventDescriptor? FindDescriptor(string elementPath, string eventName)
    {
        return descriptors.SingleOrDefault(d => d.ElementPath == elementPath && d.EventName == eventName);
    }

    public void DiscoverEventDescriptors(int rootComponentId, ComponentFrameRetriever getFrames)
    {
        descriptors.Clear();

        var rootNode = EventTreeNode.CreateRootNode();
        var queuedComponent = new QueuedComponent(rootComponentId, rootNode);
        WalkComponentFrames(getFrames: getFrames, rootComponent: queuedComponent);

        descriptors.AddRange(rootNode.EnumerateDescriptors());

        OnHandlersDiscovered?.Invoke(this, EventArgs.Empty);
    }

    private static void WalkComponentFrames(QueuedComponent rootComponent, ComponentFrameRetriever getFrames)
    {
        var remainingComponents = new Queue<QueuedComponent>([rootComponent]);

        while (remainingComponents.TryDequeue(out var item))
        {
            var scopeTracker = new ElementScopeTracker(item.ContainingNode);
            var frames = getFrames(item.ComponentId);

            for (var i = 0; i < frames.Count; ++i)
            {
                ref var frame = ref frames.Array[i];
                if (frame.FrameType is RenderTreeFrameType.Component)
                {
                    remainingComponents.Enqueue(new QueuedComponent(frame.ComponentId, scopeTracker.Current));
                    i += frame.ComponentSubtreeLength;

                    scopeTracker.TrackStep(frame.ComponentSubtreeLength);

                    continue;
                }

                if (frame.FrameType is RenderTreeFrameType.Element)
                {
                    scopeTracker.OpenScope(frame.ElementName, frame.ElementSubtreeLength);
                }

                if (frame.FrameType is RenderTreeFrameType.Attribute && frame.AttributeEventHandlerId is not 0)
                {
                    scopeTracker.Current.AddHandler(frame.AttributeEventHandlerId, item.ComponentId, frame.AttributeName);
                }

                scopeTracker.TrackStep();
            }
        }
    }

    private readonly record struct QueuedComponent(int ComponentId, EventTreeNode ContainingNode);
}
