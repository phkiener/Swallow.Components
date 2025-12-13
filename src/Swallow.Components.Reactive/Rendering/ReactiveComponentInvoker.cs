using System.Net.Mime;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Swallow.Components.Reactive.EventHandlers;

namespace Swallow.Components.Reactive.Framework;

internal sealed class ReactiveComponentInvoker(
    ReactiveComponentRenderer renderer,
    HandlerRegistration handlers,
    ILogger<ReactiveComponentInvoker> logger)
{
    public async Task InvokeAsync(Type componentType, HttpContext httpContext)
    {
        string? dispatchedEvent = null;
        string? triggeringElementPath = null;

        if (httpContext.Request.HasFormContentType)
        {
            var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
            dispatchedEvent = form["_srx-event"];
            triggeringElementPath = form["_srx-path"];
        }

        await using var writer = new StringWriter();
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            await renderer.RenderFragmentAsync(componentType).WaitAsync(httpContext.RequestAborted);
            renderer.DiscoverEventHandlers(handlers);

            httpContext.RequestAborted.ThrowIfCancellationRequested();
            if (dispatchedEvent is not null && triggeringElementPath is not null)
            {
                var descriptor = handlers.FindDescriptor(elementPath: triggeringElementPath, eventName: dispatchedEvent);
                if (descriptor is null)
                {
                    logger.LogError("Event {EventName} on element {TriggeringElementPath} did not match any event handler.", dispatchedEvent, triggeringElementPath);
                }
                else
                {
                    var dispatchOperation = renderer.DispatchEventAsync(
                        eventHandlerId: descriptor.Value.EventHandlerId,
                        fieldInfo: new EventFieldInfo { ComponentId = descriptor.Value.ComponentId },
                        eventArgs: EventArgs.Empty,
                        waitForQuiescence: true);

                    await dispatchOperation.WaitAsync(httpContext.RequestAborted);
                }
            }

            httpContext.RequestAborted.ThrowIfCancellationRequested();
            renderer.DiscoverEventHandlers(handlers);
            renderer.RenderHtml(writer);
        });

        var result = TypedResults.Text(content: writer.ToString(), contentType: MediaTypeNames.Text.Html, statusCode: StatusCodes.Status200OK);
        await result.ExecuteAsync(httpContext);
    }
}
