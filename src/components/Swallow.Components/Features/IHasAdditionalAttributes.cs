namespace Swallow.Components.Features;

/// <summary>
/// A component that can capture and render additional DOM attributes.
/// </summary>
public interface IHasAdditionalAttributes
{
    /// <summary>
    /// The attributes to render onto the DOM element.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }
}
