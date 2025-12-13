using Swallow.Components.Reactive;
using Swallow.Components.Reactive.Demo;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents()
    .AddReactiveComponents();

var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<Root>();
app.MapReactiveComponents();

app.Run();
