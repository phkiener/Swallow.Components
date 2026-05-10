using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Swallow.Components.Reactive.Test.Fakes;

namespace Swallow.Components.Reactive.Test.Invocation;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public abstract class TestWithHttpContextBase : IDisposable
{
    private readonly MemoryStream responseBody = new();
    private readonly IServiceScope serviceScope;
    private readonly ServiceProvider serviceProvider;

    protected HttpContext HttpContext { get; } = new DefaultHttpContext();

    protected TestWithHttpContextBase()
    {
        var serviceCollection = new ServiceCollection();

        var configuration = new ConfigurationBuilder().Build();
        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddSingleton<IWebHostEnvironment>(TestHostEnvironment.Instance);
        serviceCollection.AddLogging();
        serviceCollection.AddRazorComponents().AddReactiveComponents();

        serviceProvider = serviceCollection.BuildServiceProvider();
        serviceScope = serviceProvider.CreateScope();

        HttpContext.RequestServices = serviceScope.ServiceProvider;
        HttpContext.Response.Body = responseBody;
    }

    protected T Get<T>() where T : class => ActivatorUtilities.GetServiceOrCreateInstance<T>(HttpContext.RequestServices);

    protected async IAsyncEnumerable<MultipartSection> EnumerateMultipartBody()
    {
        var boundary = HttpContext.Response.GetTypedHeaders().ContentType?.Boundary.Value;
        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidOperationException("No boundary set in Content-Type header.");
        }

        var content = new MemoryStream(responseBody.ToArray());
        content.Position = 0;

        var reader = new MultipartReader(boundary, content);
        while (true)
        {
            var section = await reader.ReadNextSectionAsync();
            if (section is null)
            {
                break;
            }

            yield return section;
        }
    }

    protected async Task<MultipartSection?> FindMultipartSectionAsync(Func<MultipartSection, bool> predicate)
    {
        await foreach (var section in EnumerateMultipartBody())
        {
            if (predicate(section))
            {
                return section;

            }
        }

        return null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        responseBody.Dispose();
        serviceScope.Dispose();
        serviceProvider.Dispose();
    }
}
