namespace Swallow.Components.Reactive;

/// <summary>
/// Marker attribute for reactive components.
/// </summary>
/// <remarks>
/// This is similar to adding e.g. <c>@rendermode RenderMode.InteractiveServer</c>
/// on top of a component declaration.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class ReactiveComponentAttribute : Attribute;
