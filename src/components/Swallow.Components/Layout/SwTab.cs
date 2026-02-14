using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace Swallow.Components.Layout;

public sealed class SwTab : ComponentBase, IDisposable
{
    [Parameter]
    [EditorRequired]
    public required string Identifier { get; set; }

    [Parameter]
    [EditorRequired]
    public required RenderFragment Title { get; set; }

    [Parameter]
    [EditorRequired]
    public required RenderFragment Content { get; set; }

    [CascadingParameter]
    public required SwTabContainer Container { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // We don't render anything directly, the container tells us to render.
    }

    protected override void OnInitialized()
    {
        Container.Register(this);
    }

    public void Dispose()
    {
        Container.Unregister(this);
    }
}
