using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using NUnit.Framework;
using Swallow.Components.Reactive.Invocation;
using Swallow.Components.Reactive.Test.Utilities;

namespace Swallow.Components.Reactive.Test.Invocation;

public sealed class ReactiveComponentInvokerTest : TestWithHttpContextBase
{
    private ReactiveComponentInvoker Invoker { get; }

    public ReactiveComponentInvokerTest()
    {
        Invoker = Get<ReactiveComponentInvoker>();
    }

    [Test]
    public async Task NonPostRequest_ResultsInStatus400()
    {
        HttpContext.Request.Method = HttpMethods.Get;

        await Invoker.InvokeAsync(typeof(TestedComponent), HttpContext);
        Assert.That(HttpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task NonFormContentType_ResultsInStatus400()
    {
        HttpContext.Request.Method = HttpMethods.Post;
        HttpContext.Request.ContentType = "application/json";

        await Invoker.InvokeAsync(typeof(TestedComponent), HttpContext);
        Assert.That(HttpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task MissingHeaders_ResultsInStatus400()
    {
        HttpContext.Request.Method = HttpMethods.Post;
        HttpContext.Request.ContentType = "multipart/form-data";

        await Invoker.InvokeAsync(typeof(TestedComponent), HttpContext);
        Assert.That(HttpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    [Test]
    public async Task ValidRequest_ReturnsMultipartResponse()
    {
        HttpContext.Request.Method = HttpMethods.Post;
        HttpContext.Request.ContentType = "application/x-www-form-urlencoded";
        HttpContext.Request.Headers.Append("srx-request", "true");
        HttpContext.Request.Headers.Append("referer", "https://localhost:8000");

        await Invoker.InvokeAsync(typeof(TestedComponent), HttpContext);

        Assert.That(HttpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        Assert.That(HttpContext.Response.ContentType, Does.StartWith("multipart/mixed"));
        Assert.That(HttpContext.Response.Headers["srx-response"], Is.EqualTo("true"));
    }

    [Test]
    public async Task RenderedComponent_IsPartOfResponse()
    {
        HttpContext.Request.Method = HttpMethods.Post;
        HttpContext.Request.ContentType = "application/x-www-form-urlencoded";
        HttpContext.Request.Headers.Append("srx-request", "true");
        HttpContext.Request.Headers.Append("referer", "https://localhost:8000");

        await Invoker.InvokeAsync(typeof(TestedComponent), HttpContext);
        var section = await FindMultipartSectionAsync(s => s.ContentType is "text/html");

        Assert.That(section, Is.Not.Null);
        Assert.That(await section.ReadAsStringAsync(), Does.Contain("<span>Hello World</span>"));
    }

    [Test]
    public async Task PersistedState_IsPartOfResponse()
    {
        HttpContext.Request.Method = HttpMethods.Post;
        HttpContext.Request.ContentType = "application/x-www-form-urlencoded";
        HttpContext.Request.Headers.Append("srx-request", "true");
        HttpContext.Request.Headers.Append("referer", "https://localhost:8000");

        await Invoker.InvokeAsync(typeof(TestedComponent), HttpContext);
        var section = await FindMultipartSectionAsync(
            s => s.ContentType is "application/x-www-form-urlencoded" && s.Headers!["srx-kind"] == "state");

        Assert.That(section, Is.Not.Null);
        Assert.That(await section.ReadAsFormAsync(), Is.Not.Empty);
    }
}
