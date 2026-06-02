using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Display;

/// <summary>
/// An alert message to raise awareness for the user.
/// </summary>
public sealed partial class SwAlert : ComponentBase
{
    /// <summary>
    /// An alternative way of setting <see cref="Content"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get => Content; set => Content = value; }

    /// <summary>
    /// An optional title for this alert.
    /// </summary>
    [Parameter]
    public RenderFragment? Title { get; set; }

    /// <summary>
    /// The content to display for this alert.
    /// </summary>
    [Parameter]
    public RenderFragment? Content { get; set; }
}
