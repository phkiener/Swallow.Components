using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Reactive.Rendering.State;

/// <summary>
/// Persists all component state as <c><![CDATA[<meta>]]></c> tags.
/// </summary>
/// <remarks>
/// This component is not meant to be used directly.
/// </remarks>
public sealed partial class ComponentStateMap : ComponentBase, IDisposable
{
    private IReadOnlyDictionary<string, string> persistedState = new Dictionary<string, string>();

    [Inject]
    internal ComponentStateStore Store { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        Store.OnStatePersisted += ReloadState;
    }

    private void ReloadState(object? sender, IReadOnlyDictionary<string, string> serializedState)
    {
        persistedState = serializedState;

        StateHasChanged();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Store.OnStatePersisted -= ReloadState;
    }
}
