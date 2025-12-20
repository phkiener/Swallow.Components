namespace Swallow.Components.Reactive.Rendering.EventHandlers;

internal sealed class ElementScopeTracker(EventTreeNode currentNode)
{
    private sealed class Scope(EventTreeNode node, int frameCount)
    {
        public EventTreeNode Node { get; } = node;
        public int Remaining { get; private set; } = frameCount;

        public bool IsActive => Remaining > 0;
        public void TrackStep(int count = 1) => Remaining -= count;
    }

    private readonly List<Scope> openedScopes = [];

    public EventTreeNode Current => openedScopes.LastOrDefault()?.Node ?? currentNode;

    public void OpenScope(string tag, int componentId, int frameCount)
    {
        var node = Current.AddChild(tag, componentId);
        var scope = new Scope(node, frameCount);

        openedScopes.Add(scope);
    }

    public void TrackStep(int count = 1)
    {
        foreach (var scope in openedScopes)
        {
            scope.TrackStep(count);
        }

        openedScopes.RemoveAll(static s => !s.IsActive);
    }
}
