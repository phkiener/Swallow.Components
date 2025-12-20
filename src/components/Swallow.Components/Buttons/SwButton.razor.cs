using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Swallow.Components.Buttons;

/// <summary>
/// A plain button.
/// </summary>
public sealed partial class SwButton : ComponentBase
{
    private string buttonType => OnClick.HasDelegate ? "button" : "submit";
    private string buttonClass => string.Join(" ", EnumerateClasses());

    /// <summary>
    /// The containing <see cref="SwButtonGroup"/>, if any.
    /// </summary>
    [CascadingParameter]
    public SwButtonGroup? ButtonGroup { get; set; }

    /// <summary>
    /// The <see cref="ButtonSize"/> for this button.
    /// </summary>
    /// <remarks>
    /// Settings this overrides any setting from the parent <see cref="SwButtonGroup"/>.
    /// </remarks>
    [Parameter]
    public ButtonSize? Size { get; set; }

    /// <summary>
    /// The <see cref="ButtonVariant"/> for this button.
    /// </summary>
    /// <remarks>
    /// Settings this overrides any setting from the parent <see cref="SwButtonGroup"/>.
    /// </remarks>
    [Parameter]
    public ButtonVariant? Variant { get; set; }

    /// <summary>
    /// The <see cref="ButtonRounding"/> for this button.
    /// </summary>
    /// <remarks>
    /// Settings this overrides any setting from the parent <see cref="SwButtonGroup"/>.
    /// </remarks>
    [Parameter]
    public ButtonRounding? Rounding { get; set; }

    /// <summary>
    /// The content for the button.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    /// <summary>
    /// Callback to invoke when clicking the button.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Additional HTML attributes to render onto the button.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object?>? AdditionalAttributes { get; set; }

    private IEnumerable<string?> EnumerateClasses()
    {
        yield return "sw-button";

        var buttonSize = Size ?? ButtonGroup?.Size ?? ButtonSize.Normal;
        yield return buttonSize switch
        {
            ButtonSize.Small => "sw-button-size-small",
            ButtonSize.Smaller => "sw-button-size-smaller",
            ButtonSize.Normal => "sw-button-size-normal",
            ButtonSize.Larger => "sw-button-size-larger",
            ButtonSize.Large => "sw-button-size-large",
            _ => null
        };

        var buttonVariant = Variant ?? ButtonGroup?.Variant ?? ButtonVariant.Subtle;
        yield return buttonVariant switch
        {
            ButtonVariant.Solid => "sw-button-variant-solid",
            ButtonVariant.Subtle => "sw-button-variant-subtle",
            ButtonVariant.Outline => "sw-button-variant-outline",
            ButtonVariant.Ghost => "sw-button-variant-ghost",
            ButtonVariant.Plain => "sw-button-variant-plain",
            _ => null
        };

        var buttonRounding = Rounding ?? ButtonGroup?.Rounding ?? ButtonRounding.Normal;
        yield return buttonRounding switch
        {
            ButtonRounding.None => "sw-button-rounding-none",
            ButtonRounding.Small => "sw-button-rounding-small",
            ButtonRounding.Normal => "sw-button-rounding-normal",
            ButtonRounding.Large => "sw-button-rounding-large",
            ButtonRounding.Extra => "sw-button-rounding-extra",
            ButtonRounding.Half => "sw-button-rounding-half",
            ButtonRounding.Full => "sw-button-rounding-full",
            _ => null
        };

        if (AdditionalAttributes?.TryGetValue("class", out var extraClass) ?? false)
        {
            yield return extraClass?.ToString();
        }
    }
}
