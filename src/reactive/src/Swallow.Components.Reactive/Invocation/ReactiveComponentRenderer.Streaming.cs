using Microsoft.AspNetCore.Components.RenderTree;

namespace Swallow.Components.Reactive.Invocation;

internal sealed partial class ReactiveComponentRenderer
{
    private static readonly Task CanceledRenderTask = Task.FromCanceled(new CancellationToken(canceled: true));
    private static readonly Func<Task> EmptyCallback = static () => Task.CompletedTask;

    private Func<Task> onUpdateDisplay = EmptyCallback;

    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
    {
        return onUpdateDisplay().ContinueWith(static _ => CanceledRenderTask);
    }

    public void RegisterUpdateDisplayCallback(Func<Task> callback)
    {
        onUpdateDisplay = callback;
    }

    public void ClearUpdateDisplayCallback()
    {
        onUpdateDisplay = EmptyCallback;
    }
}
