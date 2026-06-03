using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Demo.Layout;

public sealed partial class PageHeader : ComponentBase
{
    [Parameter]
    [EditorRequired]
    public required string Title { get; set; }
}
