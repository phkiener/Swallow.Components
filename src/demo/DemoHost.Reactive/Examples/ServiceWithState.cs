using Microsoft.AspNetCore.Components;

namespace DemoHost.Reactive.Examples;

public sealed class ServiceWithState
{
    [PersistentState]
    public string Name { get; set; } = "foo";
}
