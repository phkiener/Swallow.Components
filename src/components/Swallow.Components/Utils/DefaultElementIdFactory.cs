namespace Swallow.Components.Utils;

/// <summary>
/// A default <see cref="IElementIdFactory"/> using <see cref="Guid"/>s to guarantee
/// uniqueness.
/// </summary>
public sealed class DefaultElementIdFactory : IElementIdFactory
{
    /// <inheritdoc />
    public ElementId Create(string? prefix = null)
    {
        var uniqueId = prefix is null ? $"_{Guid.NewGuid():N}" : $"_{prefix}-{Guid.NewGuid():N}";

        return new ElementId(uniqueId);
    }
}
