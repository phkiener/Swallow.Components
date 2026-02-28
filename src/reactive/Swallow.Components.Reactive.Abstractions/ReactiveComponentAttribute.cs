namespace Swallow.Components.Reactive;

/// <summary>
/// Marker attribute for reactive pages.
/// </summary>
/// <remarks>
/// This is similar to adding e.g. <code>@rendermode RenderMode.Server</code>
/// on top of a component declaration.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class ReactiveComponentAttribute : Attribute
{
    /// <summary>
    /// Instead of streaming all DOM-updates to the client, only the final DOM is sent.
    /// </summary>
    /// <remarks>
    /// This means that e.g. setting a "loading" state will <em>not</em> work.
    /// </remarks>
    public bool DisableStreaming { get; set; }
}
