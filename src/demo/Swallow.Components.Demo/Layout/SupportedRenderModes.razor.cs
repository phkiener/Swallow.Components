using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Demo.Layout;

public sealed partial class SupportedRenderModes : ComponentBase
{
    [Parameter]
    public bool Static { get; set; }

    [Parameter]
    public bool WebSocket { get; set; }

    [Parameter]
    public bool WebAssembly { get; set; }

    [Parameter]
    public bool Reactive { get; set; }
}
