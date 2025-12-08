using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Swallow.Components.Buttons;

public sealed partial class SwButton : ComponentBase
{
    private string buttonType => OnClick.HasDelegate ? "button" : "submit";
    private string buttonClass => string.Join(" ", EnumerateClasses());

    [CascadingParameter]
    public SwButtonGroup? ButtonGroup { get; set; }

    [Parameter]
    public ButtonSize? Size { get; set; }

    [Parameter]
    public ButtonVariant? Variant { get; set; }

    [Parameter]
    public ButtonRounding? Rounding { get; set; }

    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

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
