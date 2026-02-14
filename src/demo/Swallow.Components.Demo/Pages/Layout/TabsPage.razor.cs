using Microsoft.AspNetCore.Components;
using Swallow.Components.RouteGenerator;

namespace Swallow.Components.Demo.Pages.Layout;

[Route("/layout/tabs")]
[GenerateComponentRoute]
public sealed partial class TabsPage : ComponentBase
{
    [PersistentState]
    public string? ActiveTab { get; set; }

    protected override void OnInitialized()
    {
        ActiveTab ??= "first";
    }
}
