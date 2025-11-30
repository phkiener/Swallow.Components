using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Swallow.Components;

public sealed partial class SwButton : ComponentBase
{
    private string buttonType => OnClick.HasDelegate ? "button" : "submit";
    private string buttonClass => string.Join(" ", EnumerateClasses());

    [CascadingParameter]
    public SwButtonGroup? ButtonGroup { get; set; }

    [Parameter]
    public ButtonSize? Size { get; set; }

    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

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

        if (AdditionalAttributes?.TryGetValue("class", out var extraClass) ?? false)
        {
            yield return extraClass?.ToString();
        }
    }
}
