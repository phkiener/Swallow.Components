namespace Swallow.Components.Utils;

/// <summary>
/// Represents a (unique) id for an element.
/// </summary>
/// <param name="Value">The id to hold.</param>
public sealed record ElementId(string Value) : IEquatable<string>
{
    /// <inheritdoc />
    public bool Equals(string? other) => Value.Equals(other);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Cast the given <see cref="ElementId"/> to a <see cref="string"/> by unwrapping it.
    /// </summary>
    /// <param name="id">The <see cref="ElementId"/> to cast.</param>
    /// <returns>The resulting <see cref="string"/>.</returns>
    public static implicit operator string(ElementId id) => id.Value;
}
