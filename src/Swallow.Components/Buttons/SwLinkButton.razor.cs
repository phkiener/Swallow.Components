using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Buttons;

public sealed partial class SwLinkButton : ComponentBase
{
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
    [EditorRequired]
    public required string Href { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object?>? AdditionalAttributes { get; set; }

    private IEnumerable<string?> EnumerateClasses()
    {
        var buttonSize = Size ?? ButtonGroup?.Size ?? ButtonSize.Normal;
        yield return buttonSize switch
        {
            ButtonSize.Small => "size-small",
            ButtonSize.Smaller => "size-smaller",
            ButtonSize.Normal => "size-normal",
            ButtonSize.Larger => "size-larger",
            ButtonSize.Large => "size-large",
            _ => null
        };

        var buttonVariant = Variant ?? ButtonGroup?.Variant ?? ButtonVariant.Subtle;
        yield return buttonVariant switch
        {
            ButtonVariant.Solid => "variant-solid",
            ButtonVariant.Subtle => "variant-subtle",
            ButtonVariant.Outline => "variant-outline",
            ButtonVariant.Ghost => "variant-ghost",
            ButtonVariant.Plain => "variant-plain",
            _ => null
        };

        var buttonRounding = Rounding ?? ButtonGroup?.Rounding ?? ButtonRounding.Normal;
        yield return buttonRounding switch
        {
            ButtonRounding.None => "rounding-none",
            ButtonRounding.Small => "rounding-small",
            ButtonRounding.Normal => "rounding-normal",
            ButtonRounding.Large => "rounding-large",
            ButtonRounding.Extra => "rounding-extra",
            ButtonRounding.Half => "rounding-half",
            ButtonRounding.Full => "rounding-full",
            _ => null
        };

        if (AdditionalAttributes?.TryGetValue("class", out var extraClass) ?? false)
        {
            yield return extraClass?.ToString();
        }
    }
}
