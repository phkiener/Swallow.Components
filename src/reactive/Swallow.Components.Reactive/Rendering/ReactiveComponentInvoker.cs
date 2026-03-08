using System.Buffers;
using System.Collections.Concurrent;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Infrastructure;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Swallow.Components.Reactive.Rendering;
using Swallow.Components.Reactive.Rendering.EventHandlers;
using Swallow.Components.Reactive.Rendering.State;

namespace Swallow.Components.Reactive.Framework;

internal sealed class ReactiveComponentInvoker(
    ReactiveComponentRenderer renderer,
    ComponentStatePersistenceManager stateManager,
    ComponentStateStore store,
    HandlerRegistration handlers,
    ILogger<ReactiveComponentInvoker> logger)
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> componentPropertyCache = new();

    public async Task InvokeAsync(Type componentType, HttpContext context)
    {
        if (!IsValidRequest(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            return;
        }

        var form = await context.Request.ReadFormAsync();
        var hydrationResult = await RehydrateAsync(context, componentType, form);
        if (hydrationResult is RenderResult.Navigation { Location: var hydrationRedirect })
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            context.Response.Headers["srx-redirect"] = hydrationRedirect;
            context.Response.Headers["srx-response"] = "true";

            return;
        }

        if (hydrationResult is RenderResult.Error { Exception: var hydrationException })
        {
            logger.LogError(hydrationException, "Unhandled exception while rendering");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.Headers["srx-response"] = "true";

            await context.Response.WriteAsync(hydrationException.Message);

            return;
        }

        var dispatchedEvents = ReadEvent(form);
        renderer.DiscoverEventHandlers(handlers);

        var useStreaming = context.GetEndpoint()?.Metadata.GetMetadata<StreamRenderingAttribute>()?.Enabled is true;
        if (useStreaming)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = MediaTypeNames.Text.Html;
            context.Response.Headers["srx-response"] = "true";

            await using var responseWriter = new StreamedResponseWriter(
                stream: context.Response.Body,
                encoding: Encoding.UTF8,
                bufferSize: 16 * 1024,
                bytePool: ArrayPool<byte>.Shared,
                charPool: ArrayPool<char>.Shared);

            context.Response.Headers["srx-streaming-marker"] = responseWriter.Boundary;
            renderer.StreamUpdatesTo(responseWriter);

            foreach (var dispatchedEvent in dispatchedEvents)
            {
                var eventResult = await DispatchEventAsync(dispatchedEvent);
                if (eventResult is RenderResult.Navigation)
                {
                    // TODO Handle redirect
                    return;
                }

                if (eventResult is RenderResult.Error)
                {
                    // TODO Handle error
                    return;
                }
            }

            await renderer.ProcessPendingTasksAsync();

            renderer.DiscoverEventHandlers(handlers);
            await stateManager.PersistStateAsync(store, renderer);

            await renderer.ProcessPendingTasksAsync();
        }
        else
        {
            foreach (var dispatchedEvent in dispatchedEvents)
            {
                var eventResult = await DispatchEventAsync(dispatchedEvent);
                if (eventResult is RenderResult.Navigation { Location: var eventRedirect })
                {
                    context.Response.StatusCode = StatusCodes.Status204NoContent;
                    context.Response.Headers["srx-redirect"] = eventRedirect;
                    context.Response.Headers["srx-response"] = "true";

                    return;
                }

                if (eventResult is RenderResult.Error { Exception: var eventException })
                {
                    logger.LogError(eventException, "Unhandled exception while applying event");

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.Headers["srx-response"] = "true";

                    await context.Response.WriteAsync(eventException.Message);

                    return;
                }
            }

            await renderer.ProcessPendingTasksAsync();

            renderer.DiscoverEventHandlers(handlers);
            await stateManager.PersistStateAsync(store, renderer);

            await renderer.ProcessPendingTasksAsync();

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = MediaTypeNames.Text.Html;
            context.Response.Headers["srx-response"] = "true";

            await using var responseWriter = new HttpResponseStreamWriter(
                stream: context.Response.Body,
                encoding: Encoding.UTF8,
                bufferSize: 16 * 1024,
                bytePool: ArrayPool<byte>.Shared,
                charPool: ArrayPool<char>.Shared);

            await renderer.Dispatcher.InvokeAsync(() => renderer.WriteHtmlTo(responseWriter));
        }
    }

    private bool IsValidRequest(HttpRequest request)
    {
        if (!request.Headers.ContainsKey("srx-request"))
        {
            logger.LogWarning("Request to {Invoker} without {Header} received.", nameof(ReactiveComponentInvoker), "srx-request");
            return false;
        }

        if (!request.Headers.ContainsKey(HeaderNames.Referer))
        {
            logger.LogWarning("Request to {Invoker} without {Header} received.", nameof(ReactiveComponentInvoker), HeaderNames.Referer);
            return false;
        }

        return true;
    }

    private abstract record RenderResult
    {
        public sealed record Success : RenderResult;

        public sealed record Navigation(string Location) : RenderResult;

        public sealed record Error(Exception Exception) : RenderResult;
    }

    private async Task<RenderResult> RehydrateAsync(HttpContext context, Type componentType, IFormCollection form)
    {
        try
        {
            store.Initialize(form);
            await renderer.InitializeComponentServicesAsync(context, store, form);

            var parameters = ReadComponentParameters(componentType, form);
            await renderer.Dispatcher.InvokeAsync(() => renderer.RenderReactiveFragmentAsync(componentType, parameters, context));
        }
        catch (NavigationException navigation)
        {
            return new RenderResult.Navigation(navigation.Location);
        }
        catch (Exception exception)
        {
            return new RenderResult.Error(exception);
        }

        return new RenderResult.Success();
    }

    private async Task<RenderResult> DispatchEventAsync(DispatchedEvent dispatchedEvent)
    {
        var descriptor = handlers.FindDescriptor(elementPath: dispatchedEvent.Element, eventName: dispatchedEvent.Event);
        if (descriptor is null)
        {
            logger.LogError(
                "Event {EventName} on element {TriggeringElementPath} did not match any event handler.",
                dispatchedEvent.Event,
                dispatchedEvent.Element);

            return new RenderResult.Success();
        }

        var eventArgs = renderer.ParseEventArgs(descriptor.Value.EventHandlerId, dispatchedEvent.EventBody);
        var fieldInfo = new EventFieldInfo { ComponentId = descriptor.Value.ComponentId };
        if (eventArgs is ChangeEventArgs { Value: not null and var changedValue })
        {
            fieldInfo.FieldValue = changedValue;
        }

        try
        {
            await renderer.Dispatcher.InvokeAsync(async () =>
            {
                await renderer.DispatchEventAsync(
                    eventHandlerId: descriptor.Value.EventHandlerId,
                    fieldInfo: fieldInfo,
                    eventArgs: eventArgs,
                    waitForQuiescence: true);
            });
        }
        catch (NavigationException navigation)
        {
            return new RenderResult.Navigation(navigation.Location);
        }
        catch (Exception exception)
        {
            return new RenderResult.Error(exception);
        }

        return new RenderResult.Success();
    }

    private static IEnumerable<DispatchedEvent> ReadEvent(IFormCollection form)
    {
        if (form.TryGetValue(Constants.TriggeringEvent, out var values))
        {
            foreach (var value in values)
            {
                DispatchedEvent? dispatchedEvent = null;
                try
                {
                    var interaction = JsonSerializer.Deserialize<Interaction>(value ?? "");
                    if (interaction is not null)
                    {
                        dispatchedEvent = new DispatchedEvent(interaction.Trigger, interaction.EventName, interaction.EventBody);
                    }
                }
                catch (JsonException)
                {
                    // Just ignore invalid data.
                }

                if (dispatchedEvent is not null)
                {
                    yield return dispatchedEvent;
                }
            }
        }
    }

    private static Dictionary<string, object?> ReadComponentParameters(Type componentType, IFormCollection form)
    {
        var targetProperties = componentPropertyCache.GetOrAdd(componentType, ResolveComponentProperties);

        var parameters = new Dictionary<string, object?>();
        foreach (var property in targetProperties)
        {
            if (form.TryGetValue($"{Constants.ParameterPrefix}{property.Name}", out var serializedValue))
            {
                parameters[property.Name] = Serialization.DeserializeJson(serializedValue.ToString(), property.PropertyType);
            }
        }

        return parameters;
    }

    private static PropertyInfo[] ResolveComponentProperties(Type componentType)
    {
        var properties = componentType.GetProperties()
            .Where(static p => p.GetCustomAttribute<ParameterAttribute>() is not null && p is { CanRead: true, CanWrite: true })
            .ToArray();

        return properties;
    }

    private sealed record DispatchedEvent(string Element, string Event, JsonElement EventBody);

    private sealed class Interaction
    {
        [JsonPropertyName("trigger")]
        public required string Trigger { get; set; }

        [JsonPropertyName("eventName")]
        public required string EventName { get; set; }

        [JsonPropertyName("eventBody")]
        public required JsonElement EventBody { get; set; }
    }
}
