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
        // Validate;
        //   expect header srx-request
        //   expect header referer
        //   on fail: log failure, 400 bad request

        // Restore;
        //   read parameters & state
        //   read form fields?
        //   initialize component services & state store
        //   initial render
        //      on navigation: 204 srx-redirect
        //      on exception: 500
        //   discover event handlers

        // if streaming:
        //   register streaming
        //   await foreach event
        //     find listener
        //       on not found: log and skip
        //       on "broken": log and skip
        //     dispatch event
        //       on navigation: ??
        //       on exception: ??
        //   process all tasks
        //   discover handlers
        //   persist state
        //   process all tasks (to make sure we're done)
        // else:
        //   await foreach event
        //     find listener
        //       on not found: log and skip
        //       on "broken": log and skip
        //     dispatch event
        //       on navigation: 204 srx-redirect
        //       on exception: 500
        //   process all tasks
        //   discover handlers
        //   persist state
        //   process all tasks (to make sure we're done)
        //   render result

        var form = await context.Request.ReadFormAsync();
        store.Initialize(form);

        await renderer.InitializeComponentServicesAsync(context, store, form);

        try
        {
            await renderer.Dispatcher.InvokeAsync(async () =>
            {
                var parameters = ReadComponentParameters(componentType, form);
                await renderer.RenderReactiveFragmentAsync(componentType, parameters, context);
                renderer.DiscoverEventHandlers(handlers);
            });
        }
        catch (NavigationException navigation)
        {
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            context.Response.Headers["srx-redirect"] = navigation.Location;
            context.Response.Headers["srx-response"] = "true";

            await context.Response.CompleteAsync();

            return;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception while rendering");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.Headers["srx-response"] = "true";

            await context.Response.WriteAsync(exception.Message);
            await context.Response.CompleteAsync();
        }

        await using var responseWriter = new HttpResponseStreamWriter(
            stream: context.Response.Body,
            encoding: Encoding.UTF8,
            bufferSize: 16 * 1024,
            bytePool: ArrayPool<byte>.Shared,
            charPool: ArrayPool<char>.Shared);

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = MediaTypeNames.Text.Html;
        context.Response.Headers["srx-response"] = "true";

        var useStreaming = context.GetEndpoint()?.Metadata.GetMetadata<StreamRenderingAttribute>()?.Enabled is true;

        if (useStreaming)
        {
            var stream = new StreamedResponseWriter(responseWriter);
            context.Response.Headers["srx-streaming-marker"] = stream.Boundary;
            renderer.StreamUpdatesTo(stream);
        }

        foreach (var dispatchedEvent in ReadEvent(form))
        {
            var descriptor = handlers.FindDescriptor(elementPath: dispatchedEvent.Element, eventName: dispatchedEvent.Event);
            if (descriptor is null)
            {
                logger.LogError(
                    "Event {EventName} on element {TriggeringElementPath} did not match any event handler.",
                    dispatchedEvent.Event,
                    dispatchedEvent.Element);

                continue;
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
                if (useStreaming)
                {
                    // TODO: Stream the navigation?
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status204NoContent;
                    context.Response.Headers["srx-redirect"] = navigation.Location;
                    await context.Response.CompleteAsync();

                    return;
                }
            }
            catch (Exception exception)
            {
                if (useStreaming)
                {
                    // TODO: Stream the error? Abort?
                }
                else
                {
                    logger.LogError(exception, "Unhandled exception while rendering");

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync(exception.Message);
                    await context.Response.CompleteAsync();
                }
            }
        }

        await renderer.ProcessPendingTasksAsync();
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            renderer.DiscoverEventHandlers(handlers);
            await stateManager.PersistStateAsync(store, renderer);
        });

        await renderer.ProcessPendingTasksAsync();
        if (!useStreaming)
        {
            await renderer.Dispatcher.InvokeAsync(() => renderer.WriteHtmlTo(responseWriter));
        }
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
