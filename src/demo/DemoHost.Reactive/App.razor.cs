using System.Reflection;
using Microsoft.AspNetCore.Components;
using Swallow.Components.Demo.Hosting;
using Swallow.Components.Reactive;

namespace DemoHost.Reactive;

[ReactiveComponent]
public sealed class Routing : AppRoutes
{
    protected override Assembly HostAssembly { get; } = typeof(App).Assembly;
}

public sealed partial class App : ComponentBase
{
    [SupplyParameterFromQuery]
    public bool HandleErrors { get; set; } = true;
}
