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
using Swallow.Components.Reactive.Shims;

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
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.Headers["srx-response"] = "true";
        context.Response.ContentType = MediaTypeNames.Text.Html;

        await using var responseWriter = new HttpResponseStreamWriter(
            stream: context.Response.Body,
            encoding: Encoding.UTF8,
            bufferSize: 16 * 1024,
            bytePool: ArrayPool<byte>.Shared,
            charPool: ArrayPool<char>.Shared);

        var useStreaming = context.GetEndpoint()?.Metadata.GetMetadata<ReactiveComponentAttribute>()?.DisableStreaming is false;
        if (useStreaming)
        {
            var boundary = $"<!-- SRX-STREAMING-BOUNDARY {Guid.NewGuid():N} -->";
            context.Response.Headers["srx-streaming-marker"] = boundary;
            renderer.StreamUpdatesTo(responseWriter, boundary);
        }

        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            try
            {
                var form = await context.Request.ReadFormAsync();
                var dispatchedEvents = ReadEvent(form);
                var parameters = ReadComponentParameters(componentType, form);
                store.Initialize(form);

                PopulateFormDataProvider(form, context.RequestServices);
                await renderer.InitializeComponentServicesAsync(context, store);

                try
                {
                    await renderer.RenderReactiveFragmentAsync(componentType, parameters, context);
                    renderer.DiscoverEventHandlers(handlers);

                    foreach (var dispatchedEvent in dispatchedEvents)
                    {
                        var descriptor = handlers.FindDescriptor(elementPath: dispatchedEvent.Element, eventName: dispatchedEvent.Event);
                        if (descriptor is null)
                        {
                            logger.LogError("Event {EventName} on element {TriggeringElementPath} did not match any event handler.",
                                dispatchedEvent.Event, dispatchedEvent.Element);
                        }
                        else
                        {
                            var eventArgs = renderer.ParseEventArgs(descriptor.Value.EventHandlerId, dispatchedEvent.EventBody);
                            var fieldInfo = new EventFieldInfo { ComponentId = descriptor.Value.ComponentId };
                            if (eventArgs is ChangeEventArgs { Value: not null and var changedValue })
                            {
                                fieldInfo.FieldValue = changedValue;
                            }

                            await renderer.DispatchEventAsync(
                                eventHandlerId: descriptor.Value.EventHandlerId,
                                fieldInfo: fieldInfo,
                                eventArgs: eventArgs,
                                waitForQuiescence: true);
                        }
                    }
                }
                catch (NavigationException navigation)
                {
                    context.Response.StatusCode = StatusCodes.Status204NoContent;
                    context.Response.Headers["srx-redirect"] = navigation.Location;
                    context.Response.ContentType = null;

                    await context.Response.CompleteAsync();
                    return;
                }

                await stateManager.PersistStateAsync(store, renderer);

                renderer.DiscoverEventHandlers(handlers);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unhandled exception while rendering");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync(exception.Message);
                await context.Response.CompleteAsync();
            }
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

    private static void PopulateFormDataProvider(IFormCollection form, IServiceProvider serviceProvider)
    {
        // This needs to be done to initialize the SupplyParameterFromForm cascading parameter.
        var formDataProvider = HttpContextFormDataProvider.TryGet(serviceProvider);

        var formHandlerName = form["_handler"];
        if (formHandlerName.Count is 1)
        {
            formDataProvider?.SetFormData(formHandlerName[0]!, form.ToDictionary().AsReadOnly(), form.Files);
        }
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
