using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Swallow.Components.Reactive;

/// <summary>
/// Special render mode for reactive rendering.
/// </summary>
/// <remarks>
/// This is only provided for completeness; we cannot <em>actually</em> make
/// Blazor use this render mode first-class. Using <code>@rendermode RenderMode.StaticReactive</code>
/// will <em>not</em> work as expected.
/// </remarks>
public sealed class StaticReactiveRenderMode : IComponentRenderMode;

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
