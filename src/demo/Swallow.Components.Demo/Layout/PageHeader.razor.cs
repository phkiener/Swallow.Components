using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Demo.Layout;

public sealed partial class PageHeader : ComponentBase
{
    [Parameter]
    public required string Title { get; set; }
}
