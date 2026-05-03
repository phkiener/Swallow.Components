using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Swallow.Components.Reactive.Rendering.EventHandlers;

namespace Swallow.Components.Reactive.Test.Rendering.EventHandlers;

public sealed partial class HandlerRegistrationTest
{
    private readonly HandlerRegistration handlerRegistration = new();

    [Test]
    public void Initially_HasNoDescriptors()
    {
        Assert.That(handlerRegistration.Descriptors, Is.Empty);
    }

    private async Task DiscoverEventHandlersAsync(RenderFragment renderFragment)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRazorComponents();

        await using var serviceProvider = serviceCollection
            .AddSingleton<ILoggerFactory, NullLoggerFactory>()
            .AddSingleton(typeof(ILogger<>), typeof(NullLogger<>))
            .AddSingleton<IConfiguration>(new ConfigurationRoot([]))
            .AddSingleton<IWebHostEnvironment, FakeWebHostEnvironment>()
            .BuildServiceProvider();

        var renderer = new TestableRenderer(serviceProvider, NullLoggerFactory.Instance);
        await renderer.Render(renderFragment);

        handlerRegistration.DiscoverEventDescriptors(0, renderer.GetFrames);
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
