using DemoHost.WebAssembly;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Swallow.Components.Demo;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddDemoServices();

builder.RootComponents.Add<Routing>("body::after");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Logging.SetMinimumLevel(LogLevel.Warning)
    .AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information)
    .AddFilter("Swallow", LogLevel.Trace);

await builder.Build().RunAsync();
