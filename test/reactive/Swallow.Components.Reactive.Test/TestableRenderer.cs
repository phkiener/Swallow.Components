using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.HtmlRendering.Infrastructure;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Swallow.Components.Reactive.Test;

public sealed class TestableRenderer : StaticHtmlRenderer
{
    private readonly IServiceProvider serviceProvider;

    private TestableRenderer(IServiceProvider serviceProvider) : base(serviceProvider, NullLoggerFactory.Instance)
    {
        this.serviceProvider = serviceProvider;
    }

    public Task Render(RenderFragment renderFragment)
    {
        var hostParameters = new Dictionary<string, object?> { [nameof(ContentHost.Body)] = renderFragment };

        return Dispatcher.InvokeAsync(async () =>
        {
            var root = BeginRenderingComponent(typeof(ContentHost), ParameterView.FromDictionary(hostParameters));
            await root.QuiescenceTask;
        });
    }

    public static TestableRenderer Create(Action<IServiceCollection>? configure = null)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRazorComponents();
        serviceCollection.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        serviceCollection.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        serviceCollection.AddSingleton<IConfiguration>(new ConfigurationRoot([]));
        serviceCollection.AddSingleton<IWebHostEnvironment, FakeWebHostEnvironment>();
        configure?.Invoke(serviceCollection);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        return new TestableRenderer(serviceProvider);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public ArrayRange<RenderTreeFrame> GetFrames(int componentId) => GetCurrentRenderTreeFrames(componentId);

    private sealed class ContentHost : ComponentBase
    {
        [Parameter]
        [EditorRequired]
        public required RenderFragment Body { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, Body);
        }
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = "";
        public string EnvironmentName { get; set; } = "";
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } =  new NullFileProvider();
    }
}
