using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Swallow.Components.Reactive.Invocation;
using Swallow.Components.Reactive.State;
using Swallow.Components.Reactive.Test.Fakes;

namespace Swallow.Components.Reactive.Test.Invocation;

public sealed class ReactiveComponentRendererTest : IDisposable
{
    private HttpContext HttpContext { get; } = new DefaultHttpContext();
    private ServiceProvider ServiceProvider { get; }
    private IServiceScope Scope { get; }
    private ReactiveComponentRenderer Renderer { get; }

    public ReactiveComponentRendererTest()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        serviceCollection.AddSingleton<IWebHostEnvironment>(TestHostEnvironment.Instance);
        serviceCollection.AddLogging();
        serviceCollection.AddRazorComponents().AddReactiveComponents();

        ServiceProvider = serviceCollection.BuildServiceProvider();
        Scope = ServiceProvider.CreateScope();

        HttpContext.RequestServices = Scope.ServiceProvider;
        Renderer = HttpContext.RequestServices.GetRequiredService<ReactiveComponentRenderer>();
    }

    [Test]
    public async Task CommonServices_CanBeInitialized()
    {
        HttpContext.Request.Headers.Referer = "https://localhost:8000";
        HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user", "me")]));

        var store = new ReactiveComponentStore(HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>());
        await Renderer.InitializeComponentServicesAsync(HttpContext, store);

        var navigationManager = HttpContext.RequestServices.GetRequiredService<NavigationManager>();
        Assert.That(navigationManager.Uri, Is.EqualTo("https://localhost:8000/"));
        Assert.That(navigationManager.BaseUri, Is.EqualTo("https://localhost:8000/"));

        var authenticationStateProvider = HttpContext.RequestServices.GetRequiredService<AuthenticationStateProvider>();
        var authenticationState = await authenticationStateProvider.GetAuthenticationStateAsync();
        Assert.That(authenticationState.User.HasClaim("user", "me"));

        // The state manager cannot be tested here; it's covered in ReactiveComponentInvokerTest though.
    }

    public void Dispose()
    {
        ServiceProvider.Dispose();
        Scope.Dispose();
    }
}
