using Microsoft.AspNetCore.Components;

namespace Swallow.Components.Reactive.Demo;

public sealed class ExampleService
{
    [PersistentState]
    public string Name { get; set; } = "foo";
}
