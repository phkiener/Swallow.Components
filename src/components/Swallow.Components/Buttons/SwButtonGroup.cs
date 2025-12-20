using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Swallow.Components.Buttons;

/// <summary>
/// Applies common settings (<see cref="ButtonSize"/>, <see cref="ButtonRounding"/>
/// and <see cref="ButtonVariant"/>) to one or more buttons.
/// </summary>
public sealed class SwButtonGroup : ComponentBase
{
    /// <summary>
    /// The buttons to render.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// The <see cref="ButtonSize"/> to apply to all buttons, if any.
    /// </summary>
    [Parameter]
    public ButtonSize? Size { get; set; }

    /// <summary>
    /// The <see cref="ButtonVariant"/> to apply to all buttons, if any.
    /// </summary>
    [Parameter]
    public ButtonVariant? Variant { get; set; }

    /// <summary>
    /// The <see cref="ButtonRounding"/> to apply to all buttons, if any.
    /// </summary>
    [Parameter]
    public ButtonRounding? Rounding { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<SwButtonGroup>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<>.Value), this);
        builder.AddAttribute(2, nameof(CascadingValue<>.IsFixed), true);
        builder.AddAttribute(3, nameof(CascadingValue<>.ChildContent), ChildContent);
        builder.CloseComponent();
    }
}
