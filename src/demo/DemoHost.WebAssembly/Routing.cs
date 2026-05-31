using System.Reflection;
using Swallow.Components.Demo;

namespace DemoHost.WebAssembly;

public sealed class Routing : AppRoutes
{
    protected override Assembly HostAssembly { get; } = typeof(Routing).Assembly;
}
