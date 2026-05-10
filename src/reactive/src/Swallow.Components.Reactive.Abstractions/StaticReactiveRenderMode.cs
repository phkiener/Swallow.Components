using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Swallow.Components.Reactive;

/// <summary>
/// Special render mode for reactive rendering.
/// </summary>
/// <remarks>
/// This is only provided for completeness; we cannot <em>actually</em> make
/// Blazor use this render mode first-class. Using <c>@rendermode RenderMode.StaticReactive</c>
/// will <em>not</em> work as expected.
/// </remarks>
public sealed class StaticReactiveRenderMode : IComponentRenderMode
{
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is StaticReactiveRenderMode;

    /// <inheritdoc />
    public override int GetHashCode() => 1337;
}

/// <summary>
/// Extensions for <see cref="RenderMode"/>.
/// </summary>
public static class Extensions
{
    extension(RenderMode)
    {
        /// <summary>
        /// Gets an <see cref="IComponentRenderMode"/> that represents reactive server-side rendering.
        /// </summary>
        public static StaticReactiveRenderMode StaticReactive => new();
    }
}
