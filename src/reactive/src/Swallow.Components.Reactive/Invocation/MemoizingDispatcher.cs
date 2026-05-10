using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Reactive.Invocation;

internal sealed class MemoizingDispatcher(Dispatcher inner) : Dispatcher
{
    private readonly List<Task> pendingTasks = [];

    public override bool CheckAccess()
    {
        return inner.CheckAccess();
    }

    public override Task InvokeAsync(Action workItem)
    {
        var task = inner.InvokeAsync(workItem);
        pendingTasks.Add(task);

        return task;
    }

    public override Task InvokeAsync(Func<Task> workItem)
    {
        var task = inner.InvokeAsync(workItem);
        pendingTasks.Add(task);

        return task;
    }

    public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
    {
        var task = inner.InvokeAsync(workItem);
        pendingTasks.Add(task);

        return task;
    }

    public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
    {
        var task = inner.InvokeAsync(workItem);
        pendingTasks.Add(task);

        return task;
    }

    public async Task ProcessAsync()
    {
        pendingTasks.RemoveAll(IsCompleted);

        while (pendingTasks.Any())
        {
            await Task.WhenAny(pendingTasks);
            pendingTasks.RemoveAll(IsCompleted);
        }

        static bool IsCompleted(Task task) => task.IsCompleted || task.IsFaulted || task.IsCanceled;
    }
}
