using DemoHost.Reactive;
using DemoHost.Reactive.Examples;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Swallow.Components.Reactive;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents()
    .AddReactiveComponents()
    .RegisterPersistentService<ServiceWithState>(RenderMode.StaticReactive);

builder.Services.AddScoped<ServiceWithState>();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Logging.SetMinimumLevel(LogLevel.Warning)
    .AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information)
    .AddFilter("Swallow", LogLevel.Trace);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    StaticWebAssetsLoader.UseStaticWebAssets(app.Environment, app.Configuration);
}

app.UseRouting();
app.MapStaticAssets();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(Routing.AdditionalAssemblies)
    .PersistPrerenderedState();

app.MapReactiveComponents()
    .AddAdditionalAssemblies(Routing.AdditionalAssemblies);

app.Run();
