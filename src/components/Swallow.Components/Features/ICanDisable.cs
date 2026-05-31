namespace Swallow.Components.Features;

/// <summary>
/// A component that can be disabled.
/// </summary>
public interface ICanDisable
{
    /// <summary>
    /// When <see langword="true"/>, the component instance is marked as <em>disabled</em>. Any user
    /// interaction is ignored.
    /// </summary>
    public bool Disabled { get; set; }
}
