using Microsoft.AspNetCore.Components.Web;
using Swallow.Components.Reactive;
using Swallow.Components.Reactive.Demo;
using Swallow.Components.Reactive.Framework;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents()
    .AddReactiveComponents()
    .RegisterPersistentService<ExampleService>(RenderMode.StaticReactive);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddScoped<ExampleService>();

var app = builder.Build();

app.UsePathBase("/foo");
app.UseRouting();
app.MapStaticAssets();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorComponents<Root>();
app.MapReactiveComponents();

app.Run();
