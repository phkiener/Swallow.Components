using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Reactive.Rendering.EventHandlers;

/// <summary>
/// Persists all found event handlers as <c><![CDATA[<meta>]]></c> tags.
/// </summary>
/// <remarks>
/// This component is not meant to be used directly.
/// </remarks>
public sealed partial class EventHandlerMap : ComponentBase, IDisposable
{
    private readonly List<ComponentEventDescriptor> eventDescriptors = [];

    [Inject]
    internal HandlerRegistration Handlers { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        Handlers.OnHandlersDiscovered += ReloadDescriptors;
    }

    private void ReloadDescriptors(object? sender, EventArgs e)
    {
        eventDescriptors.Clear();
        eventDescriptors.AddRange(Handlers.Descriptors);

        StateHasChanged();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Handlers.OnHandlersDiscovered -= ReloadDescriptors;
    }
}
