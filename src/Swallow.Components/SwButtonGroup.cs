using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Swallow.Components;

public sealed class SwButtonGroup : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public required RenderFragment ChildContent { get; set; }

    [Parameter]
    public ButtonSize? Size { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<SwButtonGroup>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<>.Value), this);
        builder.AddAttribute(2, nameof(CascadingValue<>.IsFixed), true);
        builder.AddAttribute(3, nameof(CascadingValue<>.ChildContent), ChildContent);
        builder.CloseComponent();
    }
}
