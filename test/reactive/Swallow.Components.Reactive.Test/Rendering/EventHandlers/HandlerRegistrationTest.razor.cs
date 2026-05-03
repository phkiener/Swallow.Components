using Microsoft.AspNetCore.Components;
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
        await using var renderer = TestableRenderer.Create();
        await renderer.Render(renderFragment);

        handlerRegistration.DiscoverEventDescriptors(0, renderer.GetFrames);
    }
}
