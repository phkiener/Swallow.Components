using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Reactive;

public abstract class ReactiveComponentBase : ComponentBase
{
    [PersistentState]
    public bool FirstRenderFinished { get; set; }

    protected sealed override void OnInitialized()
    {
        OnInitialized(!FirstRenderFinished);
    }

    protected sealed override async Task OnInitializedAsync()
    {
        await OnInitializedAsync(!FirstRenderFinished);
        FirstRenderFinished = true;
    }

    protected virtual void OnInitialized(bool firstRender)
    {
        _ = firstRender;
    }

    protected virtual Task OnInitializedAsync(bool firstRender)
    {
        _ = firstRender;
        return Task.CompletedTask;
    }

    protected sealed override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
    }

    protected sealed override Task OnAfterRenderAsync(bool firstRender)
    {
        return base.OnAfterRenderAsync(firstRender);
    }
}
