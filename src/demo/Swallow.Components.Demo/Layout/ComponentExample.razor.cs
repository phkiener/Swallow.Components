using Microsoft.AspNetCore.Components;
using Swallow.Components.Demo.Utils;

namespace Swallow.Components.Demo.Layout;

public sealed partial class ComponentExample(RazorRenderer razorRenderer, MarkupRenderer markupRenderer) : ComponentBase
{
    private string renderedMarkup = "";
    private string renderedRazor = "";

    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        renderedRazor = await razorRenderer.RenderAsync(ChildContent);
        renderedMarkup = await markupRenderer.RenderAsync(ChildContent);
    }
}
