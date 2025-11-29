using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Swallow.Components;

public sealed partial class SwButton : ComponentBase
{
    private string buttonType => OnClick.HasDelegate ? "button" : "submit";

    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object?>? AdditionalAttributes { get; set; }
}
