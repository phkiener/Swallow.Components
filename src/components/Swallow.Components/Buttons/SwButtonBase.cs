using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Buttons;

/// <summary>
/// The basic definition of a button.
/// </summary>
public abstract class SwButtonBase : ComponentBase
{
    /// <summary>
    /// The CSS classes to apply for styling.
    /// </summary>
    protected string CssClass { get; private set; } = "";

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
    /// Additional HTML attributes to render onto the button.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object?>? AdditionalAttributes { get; set; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        CssClass = string.Join(" ", EnumerateCssClasses());
    }

    private IEnumerable<string> EnumerateCssClasses()
    {
        yield return "sw";
        yield return "btn";

        var effectiveSize = Size ?? ButtonGroup?.Size ?? ComponentDefaults.ButtonSize;
        yield return effectiveSize switch
        {
            ButtonSize.Small => "btn-xs",
            ButtonSize.Smaller => "btn-s",
            ButtonSize.Normal => "btn-m",
            ButtonSize.Larger => "btn-l",
            ButtonSize.Large => "btn-xl",
            _ => ""
        };

        var effectiveVariant = Variant ?? ButtonGroup?.Variant ?? ComponentDefaults.ButtonVariant;
        yield return effectiveVariant switch
        {
            ButtonVariant.Solid => "btn-vp",
            ButtonVariant.Subtle => "btn-vs",
            ButtonVariant.Outline => "btn-vo",
            ButtonVariant.Ghost => "btn-vg",
            ButtonVariant.Plain => "btn-vt",
            _ => ""
        };

        var effectiveRounding = Rounding ?? ButtonGroup?.Rounding ?? ComponentDefaults.ButtonRounding;
        yield return effectiveRounding switch
        {
            ButtonRounding.None => "btn-rn",
            ButtonRounding.Small => "btn-rs",
            ButtonRounding.Normal => "btn-rn",
            ButtonRounding.Large => "btn-rl",
            ButtonRounding.Extra => "btn-rx",
            ButtonRounding.Half => "btn-rh",
            ButtonRounding.Full => "bth-rf",
            _ => ""
        };

        if (AdditionalAttributes?.TryGetValue("class", out var extraClass) ?? false)
        {
            yield return extraClass?.ToString() ?? "";
        }
    }
}
