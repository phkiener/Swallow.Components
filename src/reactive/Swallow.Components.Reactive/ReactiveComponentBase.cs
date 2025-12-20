using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Reactive;

/// <summary>
/// Base class for reactive components that keeps track of how often
/// <see cref="ComponentBase.OnInitialized"/> was executed.
/// </summary>
public abstract class ReactiveComponentBase : ComponentBase
{
    /// <summary>
    /// <c>true</c> when this component has been initialized once.
    /// </summary>
    [PersistentState]
    public bool FirstRenderFinished { get; set; }

    /// <inheritdoc />
    protected sealed override void OnInitialized()
    {
        OnInitialized(!FirstRenderFinished);
    }

    /// <inheritdoc />
    protected sealed override async Task OnInitializedAsync()
    {
        await OnInitializedAsync(!FirstRenderFinished);
        FirstRenderFinished = true;
    }

    /// <inheritdoc cref="ComponentBase.OnInitialized" />
    /// <param name="firstRender">Whether this is the first time the component is initialized.</param>
    protected virtual void OnInitialized(bool firstRender)
    {
        _ = firstRender;
    }

    /// <inheritdoc cref="ComponentBase.OnInitializedAsync" />
    /// <param name="firstRender">Whether this is the first time the component is initialized.</param>
    protected virtual Task OnInitializedAsync(bool firstRender)
    {
        _ = firstRender;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected sealed override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
    }

    /// <inheritdoc />
    protected sealed override Task OnAfterRenderAsync(bool firstRender)
    {
        return base.OnAfterRenderAsync(firstRender);
    }
}
