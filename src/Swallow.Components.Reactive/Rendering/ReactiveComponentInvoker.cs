using System.Buffers;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Components.Infrastructure;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Swallow.Components.Reactive.EventHandlers;
using Swallow.Components.Reactive.State;

namespace Swallow.Components.Reactive.Framework;

internal sealed class ReactiveComponentInvoker(
    ReactiveComponentRenderer renderer,
    ComponentStatePersistenceManager stateManager,
    ComponentStateStore store,
    HandlerRegistration handlers,
    ILogger<ReactiveComponentInvoker> logger)
{
    private static readonly Type? FormDataProvider = typeof(IRazorComponentEndpointInvoker).Assembly.GetType("Microsoft.AspNetCore.Components.Endpoints.HttpContextFormDataProvider");
    private static MethodInfo? SetFormDataMethod;

    public async Task InvokeAsync(Type componentType, HttpContext context)
    {
        context.Response.ContentType = MediaTypeNames.Text.Html;
        await using var responseWriter = new HttpResponseStreamWriter(
            stream: context.Response.Body,
            encoding: Encoding.UTF8,
            bufferSize: 16 * 1024,
            bytePool: ArrayPool<byte>.Shared,
            charPool: ArrayPool<char>.Shared);

        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var dispatchedEvent = await ReadFormAsync(context);
            await renderer.InitializeComponentServicesAsync(context, store);

            await renderer.RenderReactiveFragmentAsync(componentType);
            renderer.DiscoverEventHandlers(handlers);

            if (dispatchedEvent is not null)
            {
                var descriptor = handlers.FindDescriptor(elementPath: dispatchedEvent.Element, eventName: dispatchedEvent.Event);
                if (descriptor is null)
                {
                    logger.LogError("Event {EventName} on element {TriggeringElementPath} did not match any event handler.", dispatchedEvent.Event, dispatchedEvent.Element);
                }
                else
                {
                    await renderer.DispatchEventAsync(
                        eventHandlerId: descriptor.Value.EventHandlerId,
                        fieldInfo: new EventFieldInfo { ComponentId = descriptor.Value.ComponentId },
                        eventArgs: EventArgs.Empty,
                        waitForQuiescence: true);
                }
            }

            await stateManager.PersistStateAsync(store, renderer);

            renderer.DiscoverEventHandlers(handlers);
            renderer.WriteHtmlTo(responseWriter);
        });
    }

    private async Task<DispatchedEvent?> ReadFormAsync(HttpContext context)
    {
        var form = await context.Request.ReadFormAsync(context.RequestAborted);

        // This needs to be done to initialize the SupplyParameterFromForm cascading parameter.
        // But Microsoft doesn't expose it, it's hidden in the EndpointHtmlRenderer. So.. we'll
        // need a bit of help to invoke it.
        SetFormDataMethod ??= FormDataProvider?.GetMethod("SetFormData");
        if (FormDataProvider is null || SetFormDataMethod is null)
        {
            logger.LogWarning("Could not retrieve HttpContextFormDataProvider via reflection; [SupplyParameterFromForm] parameters might not be available.");
        }
        else
        {
            var formHandlerName = form["_handler"];
            var formDataProvider = context.RequestServices.GetService(FormDataProvider);
            if (formDataProvider is not null && formHandlerName.Count is 1)
            {
                SetFormDataMethod.Invoke(formDataProvider, [formHandlerName[0], form.ToDictionary().AsReadOnly(), form.Files]);
            }
        }

        store.Initialize(form);

        return form.TryGetValue("_srx-event", out var eventName) && form.TryGetValue("_srx-path", out var elementPath)
            ? new DispatchedEvent(eventName.ToString(), elementPath.ToString())
            : null;
    }

    private sealed record DispatchedEvent(string Element, string Event);
}
