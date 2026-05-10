using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Swallow.Components.Reactive.Invocation;
using Swallow.Components.Reactive.Test.Fakes;

namespace Swallow.Components.Reactive.Test.Invocation;

public sealed class ReactiveComponentInvokerTest : IDisposable
{
    private MemoryStream ResponseBody { get; } = new();
    private HttpContext HttpContext { get; } = new DefaultHttpContext();
    private ServiceProvider ServiceProvider { get; }
    private IServiceScope Scope { get; }
    private ReactiveComponentInvoker Invoker { get; }

    public ReactiveComponentInvokerTest()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        serviceCollection.AddSingleton<IWebHostEnvironment>(TestHostEnvironment.Instance);
        serviceCollection.AddLogging();
        serviceCollection.AddRazorComponents().AddReactiveComponents();

        ServiceProvider = serviceCollection.BuildServiceProvider();
        Scope = ServiceProvider.CreateScope();
        HttpContext.RequestServices = Scope.ServiceProvider;
        HttpContext.Response.Body = ResponseBody;

        Invoker = HttpContext.RequestServices.GetRequiredService<ReactiveComponentInvoker>();
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
        var content = await ReadSectionAsStringAsync(section);
        Assert.That(content, Does.Contain("<span>Hello World</span>"));
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
        var content = await ReadSectionAsFormDataAsync(section);
        Assert.That(content, Is.Not.Empty);
    }

    public void Dispose()
    {
        ServiceProvider.Dispose();
    }

    private async Task<MultipartSection?> FindMultipartSectionAsync(Func<MultipartSection, bool> predicate)
    {
        ResponseBody.Position = 0;
        var boundary = HttpContext.Response.GetTypedHeaders().ContentType?.Boundary;
        if (boundary is null)
        {
            throw new InvalidOperationException("multipart/* content-type does not contain a boundary.");
        }

        var reader = new MultipartReader(boundary.Value.Value!, ResponseBody);

        MultipartSection? targetSection;
        do
        {
            targetSection = await reader.ReadNextSectionAsync();
        } while (targetSection is not null && !predicate(targetSection));

        return targetSection;
    }

    private async Task<string> ReadSectionAsStringAsync(MultipartSection section)
    {
        using var reader = new StreamReader(section.Body);
        return await reader.ReadToEndAsync();
    }

    private async Task<IFormCollection> ReadSectionAsFormDataAsync(MultipartSection section)
    {
        using var reader = new StreamReader(section.Body);
        var textContent =  await reader.ReadToEndAsync();

        var splitData = textContent.Split('&')
            .ToLookup(static t => t.Split('=')[0], static t => t.Split('=')[1])
            .ToDictionary(static g => g.Key, static g => new StringValues(g.ToArray()));

        return new FormCollection(splitData);
    }
}
