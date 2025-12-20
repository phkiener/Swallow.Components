using System.Reflection;
using Microsoft.AspNetCore.Components;
using Swallow.Components.Demo.Hosting;

namespace DemoHost.Static;

public sealed class Routing : AppRoutes
{
    protected override Assembly HostAssembly { get; } = typeof(App).Assembly;
}

public sealed partial class App : ComponentBase;
