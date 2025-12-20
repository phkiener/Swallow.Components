namespace Swallow.Components.Reactive;

/// <summary>
/// A component that describes its own key, similar to having <code>@key="Something"</code> on
/// the rendered component instance.
/// </summary>
/// <remarks>
/// This "special" key is only used in regards to reactive rendering; the Blazor diffing will not
/// respect this key.
/// </remarks>
public interface IKeyedComponent
{
    /// <summary>
    /// The key for this component.
    /// </summary>
    object? Key { get; }
}
