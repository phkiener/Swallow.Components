using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Swallow.Components.Demo;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("body::after");
builder.RootComponents.Add<HeadOutlet>("head::after");

await builder.Build().RunAsync();
