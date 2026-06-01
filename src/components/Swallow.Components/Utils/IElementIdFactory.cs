namespace Swallow.Components.Utils;

/// <summary>
/// A factory to produce unique <see cref="ElementId"/>s.
/// </summary>
public interface IElementIdFactory
{
    /// <summary>
    /// Create a new <see cref="ElementId"/>.
    /// </summary>
    /// <param name="prefix">An optional prefix for the generated id.</param>
    /// <returns>The generated <see cref="ElementId"/>.</returns>
    public ElementId Create(string? prefix = null);
}
