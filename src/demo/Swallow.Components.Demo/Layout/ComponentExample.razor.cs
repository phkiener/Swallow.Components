using Microsoft.AspNetCore.Components;
using Swallow.Components.Demo.Utils;

namespace Swallow.Components.Demo.Layout;

public sealed partial class ComponentExample(MarkupRenderer markupRenderer) : ComponentBase
{
    private string renderedMarkup = "";

    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        renderedMarkup = await markupRenderer.RenderAsMarkupAsync(ChildContent);
    }
}
