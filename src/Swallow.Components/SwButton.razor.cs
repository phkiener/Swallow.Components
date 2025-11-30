using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Swallow.Components;

public sealed partial class SwButton : ComponentBase
{
    private string buttonType => OnClick.HasDelegate ? "button" : "submit";
    private string buttonClass => string.Join(" ", EnumerateClasses());

    [Parameter]
    public ButtonSize Size { get; set; } = ButtonSize.Normal;

    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object?>? AdditionalAttributes { get; set; }

    private IEnumerable<string?> EnumerateClasses()
    {
        yield return Size switch
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
