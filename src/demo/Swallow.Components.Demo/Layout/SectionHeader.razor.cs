using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Demo.Layout;

public sealed partial class SectionHeader : ComponentBase, IDisposable
{
    [CascadingParameter]
    public MainLayout? Layout { get; set; }

    [Parameter]
    [EditorRequired]
    public required string Title { get; set; }

    [Parameter]
    [EditorRequired]
    public required string Anchor { get; set; }

    protected override void OnInitialized()
    {
        Layout?.Register(this);
    }

    public void Dispose()
    {
        Layout?.Remove(this);
    }
}
