using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using NUnit.Framework;
using Swallow.Components.Reactive.Invocation;
using Swallow.Components.Reactive.State;

namespace Swallow.Components.Reactive.Test.Invocation;

public sealed class ReactiveComponentRendererTest : TestWithHttpContextBase
{
    private ReactiveComponentRenderer Renderer { get; }

    public ReactiveComponentRendererTest() => Renderer = Get<ReactiveComponentRenderer>();

    [Test]
    public async Task CommonServices_CanBeInitialized()
    {
        HttpContext.Request.Headers.Referer = "https://localhost:8000";
        HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("user", "me")]));

        var store = Get<ReactiveComponentStore>();
        await Renderer.InitializeComponentServicesAsync(HttpContext, store);

        var navigationManager = Get<NavigationManager>();
        Assert.That(navigationManager.Uri, Is.EqualTo("https://localhost:8000/"));
        Assert.That(navigationManager.BaseUri, Is.EqualTo("https://localhost:8000/"));

        var authenticationStateProvider = Get<AuthenticationStateProvider>();
        var authenticationState = await authenticationStateProvider.GetAuthenticationStateAsync();
        Assert.That(authenticationState.User.HasClaim("user", "me"));

        // The state manager cannot be tested here; it's covered in ReactiveComponentInvokerTest though.
    }
}
