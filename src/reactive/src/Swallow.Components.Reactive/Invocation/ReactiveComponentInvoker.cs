using System.Net.Mime;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Swallow.Components.Reactive.State;

namespace Swallow.Components.Reactive.Invocation;

internal sealed class ReactiveComponentInvoker(
    ReactiveComponentRenderer renderer,
    ComponentStatePersistenceManager statePersistenceManager,
    IDataProtectionProvider dataProtectionProvider)
{
    public async Task InvokeAsync(Type componentType, HttpContext context)
    {
        if (!IsValidRequest(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var form = await context.Request.ReadFormAsync();
        var store = new ReactiveComponentStore(dataProtectionProvider);
        store.Initialize(form);

        await renderer.InitializeComponentServicesAsync(context, store);

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.Headers.Append(Headers.ResponseMarker, "true");
        await using var writer = new MultipartResponseWriter(context.Response);

        // Exceptions while invoking should be propagated to the default exception handler.
        // Except Navigation. That one is expected since it's thrown by the NavigationManager.
        try
        {
            var useStreaming = context.Request.Headers.ContainsKey(Headers.StreamingRequestMarker);
            await InvokeCoreAsync(componentType, store, writer, useStreaming);
        }
        catch (NavigationException navigation)
        {
            var content = MultipartContent.Create(MediaTypeNames.Text.Plain, navigation.Location);
            content.Headers.Append(Headers.RedirectMarker, "true");

            await writer.WriteAsync(content);
        }
    }

    private async Task InvokeCoreAsync(Type componentType, ReactiveComponentStore store, MultipartResponseWriter writer, bool useStreaming)
    {
        // TODO: Parameters
        await renderer.RehydrateComponent(componentType);
        await renderer.WaitForSettled();

        if (useStreaming)
        {
            renderer.RegisterUpdateDisplayCallback(() => UpdateDisplayAsync(writer, store));
        }

        // Step 2: Invoke event handler.

        await renderer.WaitForSettled();
        renderer.ClearUpdateDisplayCallback();

        await UpdateDisplayAsync(writer, store);
    }

    private async Task UpdateDisplayAsync(MultipartResponseWriter writer, ReactiveComponentStore store)
    {
        var markupPart = await CreateMarkupPartAsync();
        await writer.WriteAsync(markupPart);

        // TODO: Discover & write event handlers

        var statePart = await CreateStatePartAsync(store);
        await writer.WriteAsync(statePart);
    }

    private static bool IsValidRequest(HttpRequest request)
    {
        if (request.Method != HttpMethods.Post)
        {
            return false;
        }

        if (!request.HasFormContentType)
        {
            return false;
        }

        if (!request.Headers.ContainsKey(Headers.RequestMarker))
        {
            return false;
        }

        if (request.Headers.Referer.Count is 0)
        {
            return false;
        }

        return true;
    }

    private async Task<MultipartContent> CreateStatePartAsync(ReactiveComponentStore store)
    {
        await statePersistenceManager.PersistStateAsync(store, renderer);

        var serializedState = store.Serialize();
        var content = string.Join('&', serializedState.Select(static kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        var statePart = MultipartContent.Create(MediaTypeNames.Application.FormUrlEncoded, content);
        statePart.Headers.Append(Headers.ResponsePartIdentifier, Headers.ResponsePartState);

        return statePart;
    }

    private async Task<MultipartContent> CreateMarkupPartAsync()
    {
        var markup = await renderer.RenderMarkupAsync();
        return MultipartContent.Create(MediaTypeNames.Text.Html, markup);
    }
}
